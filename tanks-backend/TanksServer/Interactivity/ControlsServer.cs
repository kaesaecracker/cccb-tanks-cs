using System.Net.WebSockets;

namespace TanksServer.Interactivity;

internal sealed class ControlsServer(
    ILogger<ControlsServer> logger,
    ILoggerFactory loggerFactory
) : WebsocketServer<ControlsServerConnection>(logger)
{
    public Task HandleClientAsync(WebSocket ws, Player player)
    {
        logger.LogDebug("control client connected {}", player.Name);
        var clientLogger = loggerFactory.CreateLogger<ControlsServerConnection>();
        var sock = new ControlsServerConnection(ws, clientLogger, player);
        return HandleClientAsync(sock);
    }
}
