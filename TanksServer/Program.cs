using System.IO;
using DisplayCommands;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
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

        var clientFileProvider = new PhysicalFileProvider(Path.Combine(app.Environment.ContentRootPath, "client"));
        app.UseDefaultFiles(new DefaultFilesOptions { FileProvider = clientFileProvider });
        app.UseStaticFiles(new StaticFileOptions { FileProvider = clientFileProvider });

        Endpoints.MapEndpoints(app);

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

        builder.Services.Configure<HostConfiguration>(builder.Configuration.GetSection("Host"));
        var hostConfiguration = builder.Configuration.GetSection("Host").Get<HostConfiguration>();
        if (hostConfiguration == null)
            throw new InvalidOperationException("'Host' configuration missing");

        builder.Services.AddSingleton<MapService>();
        builder.Services.AddSingleton<MapEntityManager>();
        builder.Services.AddSingleton<ControlsServer>();
        builder.Services.AddSingleton<PlayerServer>();
        builder.Services.AddSingleton<ClientScreenServer>();
        builder.Services.AddSingleton<TankSpawnQueue>();

        builder.Services.AddHostedService<GameTickWorker>();
        builder.Services.AddHostedService(sp => sp.GetRequiredService<ControlsServer>());
        builder.Services.AddHostedService(sp => sp.GetRequiredService<ClientScreenServer>());

        builder.Services.AddSingleton<ITickStep, MoveBullets>();
        builder.Services.AddSingleton<ITickStep, CollideBullets>();
        builder.Services.AddSingleton<ITickStep, RotateTanks>();
        builder.Services.AddSingleton<ITickStep, MoveTanks>();
        builder.Services.AddSingleton<ITickStep, ShootFromTanks>();
        builder.Services.AddSingleton<ITickStep, CollectPowerUp>();
        builder.Services.AddSingleton<ITickStep>(sp => sp.GetRequiredService<TankSpawnQueue>());
        builder.Services.AddSingleton<ITickStep, SpawnPowerUp>();
        builder.Services.AddSingleton<ITickStep, GeneratePixelsTickStep>();

        builder.Services.AddSingleton<IDrawStep, DrawMapStep>();
        builder.Services.AddSingleton<IDrawStep, DrawPowerUpsStep>();
        builder.Services.AddSingleton<IDrawStep, DrawTanksStep>();
        builder.Services.AddSingleton<IDrawStep, DrawBulletsStep>();

        builder.Services.AddSingleton<IFrameConsumer, ClientScreenServer>(sp =>
            sp.GetRequiredService<ClientScreenServer>());

        builder.Services.Configure<TanksConfiguration>(
            builder.Configuration.GetSection("Tanks"));
        builder.Services.Configure<PlayersConfiguration>(
            builder.Configuration.GetSection("Players"));
        builder.Services.Configure<GameRules>(builder.Configuration.GetSection("GameRules"));

        if (hostConfiguration.EnableServicePointDisplay)
        {
            builder.Services.AddSingleton<IFrameConsumer, SendToServicePointDisplay>();
            builder.Services.AddDisplay(builder.Configuration.GetSection("ServicePointDisplay"));
        }

        var app = builder.Build();

        app.UseCors();
        app.UseWebSockets();
        app.UseHttpLogging();

        return app;
    }
}
