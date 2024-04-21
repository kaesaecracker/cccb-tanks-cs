using System.Net.WebSockets;
using DisplayCommands;
using TanksServer.Graphics;

namespace TanksServer.Interactivity;

internal sealed class ClientScreenServer(
    ILogger<ClientScreenServer> logger,
    ILoggerFactory loggerFactory,
    IOptions<HostConfiguration> hostConfig
) : WebsocketServer<ClientScreenServerConnection>(logger), IFrameConsumer
{
    private readonly TimeSpan _minFrameTime = TimeSpan.FromMilliseconds(hostConfig.Value.ClientDisplayMinFrameTimeMs);

    public Task HandleClientAsync(WebSocket socket, Guid? playerGuid)
        => base.HandleClientAsync(new(
            socket,
            loggerFactory.CreateLogger<ClientScreenServerConnection>(),
            _minFrameTime,
            playerGuid
        ));

    public Task OnFrameDoneAsync(GamePixelGrid gamePixelGrid, PixelGrid observerPixels)
        => ParallelForEachConnectionAsync(c => c.SendAsync(observerPixels, gamePixelGrid));
}
