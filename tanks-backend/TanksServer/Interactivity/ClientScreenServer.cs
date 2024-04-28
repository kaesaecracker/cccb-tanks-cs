using System.Net.WebSockets;
using DisplayCommands;
using TanksServer.Graphics;

namespace TanksServer.Interactivity;

internal sealed class ClientScreenServer(
    ILogger<ClientScreenServer> logger,
    ILoggerFactory loggerFactory
) : WebsocketServer<ClientScreenServerConnection>(logger), IFrameConsumer
{
    public Task HandleClientAsync(WebSocket socket, string? player)
        => base.HandleClientAsync(new(
            socket,
            loggerFactory.CreateLogger<ClientScreenServerConnection>(),
            player
        ));

    public ValueTask OnFrameDoneAsync(GamePixelGrid gamePixelGrid, PixelGrid observerPixels)
        => ParallelForEachConnectionAsync(c => c.OnGameTickAsync(observerPixels, gamePixelGrid).AsTask());
}
