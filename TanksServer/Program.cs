using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace TanksServer;

internal static class Program
{
    public static async Task Main(string[] args)
    {
        var app = Configure(args);

        var display = app.Services.GetRequiredService<ServicePointDisplay>();
        var mapDrawer = app.Services.GetRequiredService<MapDrawer>();

        var buffer = mapDrawer.CreateGameFieldPixelBuffer();
        mapDrawer.DrawInto(buffer);
        await display.Send(buffer);

        app.UseWebSockets();

        app.Map("/screen", async context =>
        {
            if (!context.WebSockets.IsWebSocketRequest)
                return;
            using var ws = await context.WebSockets.AcceptWebSocketAsync();
        });

        await app.RunAsync();
    }

    private static WebApplication Configure(string[] args)
    {
        var builder = WebApplication.CreateSlimBuilder(args);

        builder.Services.AddSingleton<ServicePointDisplay>();
        builder.Services.AddSingleton<MapService>();
        builder.Services.AddSingleton<MapDrawer>();

        return builder.Build();
    }
}
