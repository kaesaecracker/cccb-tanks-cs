using System.Net.WebSockets;
using DisplayCommands;
using TanksServer.Graphics;

namespace TanksServer.Interactivity;

internal sealed class ClientScreenServer(
    ILogger<ClientScreenServer> logger,
    ILoggerFactory loggerFactory
) : WebsocketServer<ClientScreenServerConnection>(logger),
    IFrameConsumer
{
    public Task HandleClientAsync(WebSocket socket, Player? player)
        => base.HandleClientAsync(new ClientScreenServerConnection(
            socket,
            loggerFactory.CreateLogger<ClientScreenServerConnection>(),
            player
        ));

    public async Task OnFrameDoneAsync(GamePixelGrid gamePixelGrid, PixelGrid observerPixels)
        => await ParallelForEachConnectionAsync(c => c.OnGameTickAsync(observerPixels, gamePixelGrid).AsTask());
}
