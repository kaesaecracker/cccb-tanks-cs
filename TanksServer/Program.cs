using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using TanksServer.DrawSteps;
using TanksServer.Helpers;
using TanksServer.Models;
using TanksServer.Servers;
using TanksServer.Services;
using TanksServer.TickSteps;

namespace TanksServer;

internal static class Program
{
    public static async Task Main(string[] args)
    {
        var app = Configure(args);

        app.UseCors();
        app.UseWebSockets();

        var clientScreenServer = app.Services.GetRequiredService<ClientScreenServer>();
        var playerService = app.Services.GetRequiredService<PlayerServer>();
        var controlsServer = app.Services.GetRequiredService<ControlsServer>();

        var clientFileProvider = new PhysicalFileProvider(Path.Combine(app.Environment.ContentRootPath, "client"));
        app.UseDefaultFiles(new DefaultFilesOptions { FileProvider = clientFileProvider });
        app.UseStaticFiles(new StaticFileOptions { FileProvider = clientFileProvider });

        app.MapGet("/player", playerService.GetOrAdd);

        app.Map("/screen", async context =>
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }

            using var ws = await context.WebSockets.AcceptWebSocketAsync();
            await clientScreenServer.HandleClient(ws);
        });

        app.Map("/controls", async (HttpContext context, [FromQuery] Guid playerId) =>
        {
            if (!context.WebSockets.IsWebSocketRequest)
                return Results.BadRequest();

            if (!playerService.TryGet(playerId, out var player))
                return Results.NotFound();

            using var ws = await context.WebSockets.AcceptWebSocketAsync();
            await controlsServer.HandleClient(ws, player);
            return Results.Empty;
        });

        await app.RunAsync();
    }

    private static WebApplication Configure(string[] args)
    {
        var builder = WebApplication.CreateSlimBuilder(args);

        builder.Services.AddCors(options => options
            .AddDefaultPolicy(policy => policy
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowAnyOrigin())
        );

        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.TypeInfoResolverChain.Insert(0, new AppSerializerContext());
        });

        builder.Services.AddOptions();

        builder.Services.AddSingleton<MapService>();
        builder.Services.AddSingleton<BulletManager>();
        builder.Services.AddSingleton<TankManager>();
        builder.Services.AddSingleton<SpawnNewTanks>();
        builder.Services.AddSingleton<ControlsServer>();
        builder.Services.AddSingleton<PlayerServer>();
        builder.Services.AddSingleton<ClientScreenServer>();
        builder.Services.AddSingleton<LastFinishedFrameProvider>();
        builder.Services.AddSingleton<SpawnQueueProvider>();

        builder.Services.AddHostedService<GameTickService>();
        builder.Services.AddHostedService(sp => sp.GetRequiredService<ControlsServer>());
        builder.Services.AddHostedService(sp => sp.GetRequiredService<ClientScreenServer>());

        builder.Services.AddSingleton<ITickStep, MoveBullets>();
        builder.Services.AddSingleton<ITickStep, CollideBulletsWithTanks>();
        builder.Services.AddSingleton<ITickStep, CollideBulletsWithMap>();
        builder.Services.AddSingleton<ITickStep, RotateTanks>();
        builder.Services.AddSingleton<ITickStep, MoveTanks>();
        builder.Services.AddSingleton<ITickStep, ShootFromTanks>();
        builder.Services.AddSingleton<ITickStep>(sp => sp.GetRequiredService<SpawnNewTanks>());
        builder.Services.AddSingleton<ITickStep, DrawStateToFrame>();
        builder.Services.AddSingleton<ITickStep, SendToServicePointDisplay>();
        builder.Services.AddSingleton<ITickStep, SendToClientScreen>();

        builder.Services.AddSingleton<IDrawStep, MapDrawer>();
        builder.Services.AddSingleton<IDrawStep, TankDrawer>();
        builder.Services.AddSingleton<IDrawStep, BulletDrawer>();

        builder.Services.Configure<ServicePointDisplayConfiguration>(
            builder.Configuration.GetSection("ServicePointDisplay"));

        return builder.Build();
    }
}
