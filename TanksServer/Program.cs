using System.IO;
using DisplayCommands;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using TanksServer.GameLogic;
using TanksServer.Graphics;
using TanksServer.Interactivity;

namespace TanksServer;

public static class Program
{
    public static void Main(string[] args)
    {
        var app = Configure(args);

        var clientScreenServer = app.Services.GetRequiredService<ClientScreenServer>();
        var playerService = app.Services.GetRequiredService<PlayerServer>();
        var controlsServer = app.Services.GetRequiredService<ControlsServer>();

        var clientFileProvider = new PhysicalFileProvider(Path.Combine(app.Environment.ContentRootPath, "client"));
        app.UseDefaultFiles(new DefaultFilesOptions { FileProvider = clientFileProvider });
        app.UseStaticFiles(new StaticFileOptions { FileProvider = clientFileProvider });

        app.MapPost("/player", (string name, Guid id) =>
        {
            var player = playerService.GetOrAdd(name, id);
            return player != null
                ? Results.Ok(player.Id)
                : Results.Unauthorized();
        });
        app.MapGet("/player", ([FromQuery] Guid id) =>
            playerService.TryGet(id, out var foundPlayer)
                ? Results.Ok((object?)foundPlayer)
                : Results.NotFound()
        );

        app.MapGet("/scores", () => playerService.GetAll());

        app.Map("/screen", async (HttpContext context) =>
        {
            if (!context.WebSockets.IsWebSocketRequest)
                return Results.BadRequest();

            using var ws = await context.WebSockets.AcceptWebSocketAsync();
            await clientScreenServer.HandleClient(ws);
            return null;
        });

        app.Map("/controls", async (HttpContext context, [FromQuery] Guid playerId) =>
        {
            if (!context.WebSockets.IsWebSocketRequest)
                return Results.BadRequest();

            if (!playerService.TryGet(playerId, out var player))
                return Results.NotFound();

            using var ws = await context.WebSockets.AcceptWebSocketAsync();
            await controlsServer.HandleClient(ws, player);
            return null;
        });

        app.Run();
    }

    private static WebApplication Configure(string[] args)
    {
        var builder = WebApplication.CreateSlimBuilder(args);

        builder.Logging.AddSimpleConsole(options =>
        {
            options.SingleLine = true;
            options.IncludeScopes = true;
            options.TimestampFormat = "HH:mm:ss ";
        });

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

        builder.Services.AddHttpLogging(_ => { });

        builder.Services.AddSingleton<MapService>();
        builder.Services.AddSingleton<BulletManager>();
        builder.Services.AddSingleton<TankManager>();
        builder.Services.AddSingleton<ControlsServer>();
        builder.Services.AddSingleton<PlayerServer>();
        builder.Services.AddSingleton<ClientScreenServer>();
        builder.Services.AddSingleton<LastFinishedFrameProvider>();
        builder.Services.AddSingleton<SpawnQueue>();

        builder.Services.AddHostedService<GameTickWorker>();
        builder.Services.AddHostedService(sp => sp.GetRequiredService<ControlsServer>());
        builder.Services.AddHostedService(sp => sp.GetRequiredService<ClientScreenServer>());

        builder.Services.AddSingleton<ITickStep, MoveBullets>();
        builder.Services.AddSingleton<ITickStep, CollideBulletsWithTanks>();
        builder.Services.AddSingleton<ITickStep, CollideBulletsWithMap>();
        builder.Services.AddSingleton<ITickStep, RotateTanks>();
        builder.Services.AddSingleton<ITickStep, MoveTanks>();
        builder.Services.AddSingleton<ITickStep, ShootFromTanks>();
        builder.Services.AddSingleton<ITickStep, SpawnNewTanks>();
        builder.Services.AddSingleton<ITickStep, GeneratePixelsTickStep>();
        builder.Services.AddSingleton<ITickStep, SendToServicePointDisplay>();
        builder.Services.AddSingleton<ITickStep, SendToClientScreen>();

        builder.Services.AddSingleton<IDrawStep, DrawMapStep>();
        builder.Services.AddSingleton<IDrawStep, DrawTanksStep>();
        builder.Services.AddSingleton<IDrawStep, DrawBulletsStep>();

        builder.Services.Configure<TanksConfiguration>(
            builder.Configuration.GetSection("Tanks"));
        builder.Services.Configure<PlayersConfiguration>(
            builder.Configuration.GetSection("Players"));
        builder.Services.AddDisplay(builder.Configuration.GetSection("ServicePointDisplay"));

        var app = builder.Build();

        app.UseCors();
        app.UseWebSockets();
        app.UseHttpLogging();

        return app;
    }
}
