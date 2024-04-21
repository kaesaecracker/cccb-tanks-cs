using System.Net.WebSockets;

namespace TanksServer.Interactivity;

internal sealed class ControlsServer(
    ILogger<ControlsServer> logger,
    ILoggerFactory loggerFactory
) : WebsocketServer<ControlsServerConnection>(logger)
{
    public async Task HandleClientAsync(WebSocket ws, Player player)
    {
        logger.LogDebug("control client connected {}", player.Id);
        var clientLogger = loggerFactory.CreateLogger<ControlsServerConnection>();
        var sock = new ControlsServerConnection(ws, clientLogger, player);
        await AddConnection(sock);
        await sock.ReceiveAsync();
        await RemoveConnection(sock);
    }
}
