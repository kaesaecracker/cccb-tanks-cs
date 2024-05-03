using System.Net.WebSockets;
using DisplayCommands;
using TanksServer.Graphics;

namespace TanksServer.Interactivity;

internal sealed class ClientScreenServer(
    ILogger<ClientScreenServer> logger,
    ILoggerFactory loggerFactory,
    BufferPool bufferPool
) : WebsocketServer<ClientScreenServerConnection>(logger),
    IFrameConsumer
{
    public Task HandleClientAsync(WebSocket socket, Player? player)
    {
        var connection = new ClientScreenServerConnection(
            socket,
            loggerFactory.CreateLogger<ClientScreenServerConnection>(),
            player,
            bufferPool
        );
        return base.HandleClientAsync(connection);
    }

    public Task OnFrameDoneAsync(GamePixelGrid gamePixelGrid, PixelGrid observerPixels)
        => Connections.Select(c => c.OnGameTickAsync(observerPixels, gamePixelGrid))
            .WhenAll();
}
