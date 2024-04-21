using System.Net.WebSockets;
using Microsoft.Extensions.Hosting;

namespace TanksServer.Interactivity;

internal sealed class ControlsServer(ILogger<ControlsServer> logger, ILoggerFactory loggerFactory)
    : IHostedLifecycleService
{
    private readonly List<ControlsServerConnection> _connections = [];

    public Task StoppingAsync(CancellationToken cancellationToken)
    {
        return Task.WhenAll(_connections.Select(c => c.CloseAsync()));
    }

    public Task HandleClient(WebSocket ws, Player player)
    {
        logger.LogDebug("control client connected {}", player.Id);
        var clientLogger = loggerFactory.CreateLogger<ControlsServerConnection>();
        var sock = new ControlsServerConnection(ws, clientLogger, this, player);
        _connections.Add(sock);
        return sock.Done;
    }

    private void Remove(ControlsServerConnection connection) => _connections.Remove(connection);

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StartedAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StartingAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private sealed class ControlsServerConnection
    {
        private readonly ByteChannelWebSocket _binaryWebSocket;
        private readonly ILogger<ControlsServerConnection> _logger;
        private readonly Player _player;
        private readonly ControlsServer _server;

        public ControlsServerConnection(WebSocket socket, ILogger<ControlsServerConnection> logger,
            ControlsServer server, Player player)
        {
            _logger = logger;
            _server = server;
            _player = player;
            _binaryWebSocket = new ByteChannelWebSocket(socket, logger, 2);
            Done = ReceiveAsync();
        }

        public Task Done { get; }

        private async Task ReceiveAsync()
        {
            await foreach (var buffer in _binaryWebSocket.ReadAllAsync())
            {
                var type = (MessageType)buffer.Span[0];
                var control = (InputType)buffer.Span[1];

                _logger.LogTrace("player input {} {} {}", _player.Id, type, control);

                var isEnable = type switch
                {
                    MessageType.Enable => true,
                    MessageType.Disable => false,
                    _ => throw new ArgumentException("invalid message type")
                };

                _player.LastInput = DateTime.Now;

                switch (control)
                {
                    case InputType.Forward:
                        _player.Controls.Forward = isEnable;
                        break;
                    case InputType.Backward:
                        _player.Controls.Backward = isEnable;
                        break;
                    case InputType.Left:
                        _player.Controls.TurnLeft = isEnable;
                        break;
                    case InputType.Right:
                        _player.Controls.TurnRight = isEnable;
                        break;
                    case InputType.Shoot:
                        _player.Controls.Shoot = isEnable;
                        break;
                    default:
                        throw new ArgumentException("invalid control type");
                }
            }

            _server.Remove(this);
        }

        public Task CloseAsync()
        {
            return _binaryWebSocket.CloseAsync();
        }

        private enum MessageType : byte
        {
            Enable = 0x01,
            Disable = 0x02
        }

        private enum InputType : byte
        {
            Forward = 0x01,
            Backward = 0x02,
            Left = 0x03,
            Right = 0x04,
            Shoot = 0x05
        }
    }
}
