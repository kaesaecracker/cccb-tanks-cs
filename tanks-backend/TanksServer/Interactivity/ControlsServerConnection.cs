using System.Net.WebSockets;

namespace TanksServer.Interactivity;

internal sealed class ControlsServerConnection : IWebsocketServerConnection
{
    private readonly ByteChannelWebSocket _binaryWebSocket;
    private readonly ILogger<ControlsServerConnection> _logger;
    private readonly Player _player;

    public ControlsServerConnection(WebSocket socket, ILogger<ControlsServerConnection> logger, Player player)
    {
        _logger = logger;
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
    }

    public Task CloseAsync() => _binaryWebSocket.CloseAsync();

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
