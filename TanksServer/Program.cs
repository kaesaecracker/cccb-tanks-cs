using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

namespace TanksServer;

internal static class Program
{
    public static async Task Main(string[] args)
    {
        var app = Configure(args);

        var clientScreenServer = app.Services.GetRequiredService<ClientScreenServer>();

        var clientFileProvider = new PhysicalFileProvider(Path.Combine(app.Environment.ContentRootPath, "client"));
        app.UseDefaultFiles(new DefaultFilesOptions { FileProvider = clientFileProvider });
        app.UseStaticFiles(new StaticFileOptions { FileProvider = clientFileProvider });

        app.UseWebSockets();
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

        await app.RunAsync();
    }

    private static WebApplication Configure(string[] args)
    {
        var builder = WebApplication.CreateSlimBuilder(args);

        builder.Services.AddSingleton<ServicePointDisplay>();
        builder.Services.AddSingleton<MapService>();
        
        builder.Services.AddSingleton<MapDrawer>();
        builder.Services.AddSingleton<ITickStep, MapDrawer>(sp => sp.GetRequiredService<MapDrawer>());
        
        builder.Services.AddSingleton<ClientScreenServer>();
        builder.Services.AddHostedService<ClientScreenServer>(sp => sp.GetRequiredService<ClientScreenServer>());
        builder.Services.AddSingleton<ITickStep, ClientScreenServer>(sp => sp.GetRequiredService<ClientScreenServer>());

        builder.Services.AddHostedService<GameTickService>();
        
        return builder.Build();
    }
}
