using System.Net.WebSockets;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TanksServer.Helpers;

namespace TanksServer;

internal sealed class ControlsServer(ILogger<ControlsServer> logger, ILoggerFactory loggerFactory)
    : IHostedLifecycleService
{
    private readonly List<ControlsServerConnection> _connections = new();

    public Task HandleClient(WebSocket ws, Player player)
    {
        logger.LogDebug("control client connected {}", player.Id);
        var clientLogger = loggerFactory.CreateLogger<ControlsServerConnection>();
        var sock = new ControlsServerConnection(ws, clientLogger, this, player);
        _connections.Add(sock);
        return sock.Done;
    }

    public Task StoppingAsync(CancellationToken cancellationToken)
    {
        return Task.WhenAll(_connections.Select(c => c.CloseAsync()));
    }

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StartedAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StartingAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private void Remove(ControlsServerConnection connection)
    {
        _connections.Remove(connection);
    }

    private sealed class ControlsServerConnection(WebSocket socket, ILogger logger, ControlsServer server,
            Player player)
        : EasyWebSocket(socket, logger, new byte[2])
    {
        private enum MessageType : byte
        {
            Enable = 0x01,
            Disable = 0x02,
        }

        private enum InputType : byte
        {
            Forward = 0x01,
            Backward = 0x02,
            Left = 0x03,
            Right = 0x04,
            Shoot = 0x05
        }

        protected override Task ReceiveAsync(ArraySegment<byte> buffer)
        {
            var type = (MessageType)buffer[0];
            var control = (InputType)buffer[1];
            
            logger.LogTrace("player input {} {} {}", player.Id, type, control);

            var isEnable = type switch
            {
                MessageType.Enable => true,
                MessageType.Disable => false,
                _ => throw new ArgumentException("invalid message type")
            };

            switch (control)
            {
                case InputType.Forward:
                    player.Controls.Forward = isEnable;
                    break;
                case InputType.Backward:
                    player.Controls.Backward = isEnable;
                    break;
                case InputType.Left:
                    player.Controls.TurnLeft = isEnable;
                    break;
                case InputType.Right:
                    player.Controls.TurnRight = isEnable;
                    break;
                case InputType.Shoot:
                    player.Controls.Shoot = isEnable;
                    break;
                default:
                    throw new ArgumentException("invalid control type");
            }

            return Task.CompletedTask;
        }

        protected override Task ClosingAsync()
        {
            server.Remove(this);
            return Task.CompletedTask;
        }
    }
}
