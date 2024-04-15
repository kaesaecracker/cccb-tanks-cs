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

internal sealed record class NameId(string Name, Guid Id);

public static class Program
{
    public static void Main(string[] args)
    {
        var app = Configure(args);

        var clientScreenServer = app.Services.GetRequiredService<ClientScreenServer>();
        var playerService = app.Services.GetRequiredService<PlayerServer>();
        var controlsServer = app.Services.GetRequiredService<ControlsServer>();
        var mapService = app.Services.GetRequiredService<MapService>();

        var clientFileProvider = new PhysicalFileProvider(Path.Combine(app.Environment.ContentRootPath, "client"));
        app.UseDefaultFiles(new DefaultFilesOptions { FileProvider = clientFileProvider });
        app.UseStaticFiles(new StaticFileOptions { FileProvider = clientFileProvider });

        app.MapPost("/player", (string name, Guid id) =>
        {
            name = name.Trim().ToUpperInvariant();
            if (name == string.Empty)
                return Results.BadRequest("name cannot be blank");
            if (name.Length > 12)
                return Results.BadRequest("name too long");

            var player = playerService.GetOrAdd(name, id);
            return player != null
                ? Results.Ok(new NameId(player.Name, player.Id))
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
            return Results.Empty;
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

        app.MapGet("/map", () =>
        {
            var dict = mapService.All
                .Select((m, i) => (m.Name, i))
                .ToDictionary(pair => pair.i, pair => pair.Name);
            return dict;
        });

        app.MapPost("/map", ([FromQuery] int index) =>
        {
            if (index < 0 || index >= mapService.All.Length)
                return Results.NotFound("index does not exist");
            mapService.Current = mapService.All[index];
            return Results.Ok();
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

        builder.Services.AddSingleton<IDrawStep, DrawMapStep>();
        builder.Services.AddSingleton<IDrawStep, DrawTanksStep>();
        builder.Services.AddSingleton<IDrawStep, DrawBulletsStep>();

        builder.Services.AddSingleton<IFrameConsumer, SendToServicePointDisplay>();
        builder.Services.AddSingleton<IFrameConsumer, ClientScreenServer>(sp =>
            sp.GetRequiredService<ClientScreenServer>());

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
