using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using TanksServer.Helpers;
using TanksServer.Services;

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

        builder.Services.AddSingleton<ServicePointDisplay>();
        builder.Services.AddSingleton<MapService>();
        builder.Services.AddSingleton<TankManager>();

        builder.Services.AddHostedService<GameTickService>();

        builder.Services.AddSingleton<SpawnQueue>();
        builder.Services.AddSingleton<ITickStep>(sp => sp.GetRequiredService<SpawnQueue>());

        builder.Services.AddSingleton<PixelDrawer>();
        builder.Services.AddSingleton<ITickStep, PixelDrawer>(sp => sp.GetRequiredService<PixelDrawer>());

        builder.Services.AddSingleton<ClientScreenServer>();
        builder.Services.AddHostedService(sp => sp.GetRequiredService<ClientScreenServer>());
        builder.Services.AddSingleton<ITickStep, ClientScreenServer>(sp => sp.GetRequiredService<ClientScreenServer>());

        builder.Services.AddSingleton<ControlsServer>();
        builder.Services.AddHostedService(sp => sp.GetRequiredService<ControlsServer>());
        
        builder.Services.AddSingleton<PlayerServer>();

        return builder.Build();
    }
}
