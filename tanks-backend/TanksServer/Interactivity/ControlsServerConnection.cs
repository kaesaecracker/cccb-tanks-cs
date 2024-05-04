using System.Net.WebSockets;

namespace TanksServer.Interactivity;

internal sealed class ControlsServerConnection : WebsocketServerConnection
{
    private readonly Player _player;

    public ControlsServerConnection(WebSocket socket,
        ILogger<ControlsServerConnection> logger,
        Player player) : base(logger, new ByteChannelWebSocket(socket, logger, 2))
    {
        _player = player;
        _player.IncrementConnectionCount();
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

    protected override ValueTask HandleMessageAsync(Memory<byte> buffer)
    {
        var type = (MessageType)buffer.Span[0];
        var control = (InputType)buffer.Span[1];

        Logger.LogTrace("player input {} {} {}", _player.Name, type, control);

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

        return ValueTask.CompletedTask;
    }

    public override void Dispose() => _player.DecrementConnectionCount();
}
