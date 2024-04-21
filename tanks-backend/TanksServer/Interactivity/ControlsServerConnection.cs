using System.Net.WebSockets;

namespace TanksServer.Interactivity;

internal sealed class ControlsServerConnection(
    WebSocket socket,
    ILogger<ControlsServerConnection> logger,
    Player player
) : WebsocketServerConnection(logger, new ByteChannelWebSocket(socket, logger, 2))
{
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

    protected override void HandleMessage(Memory<byte> buffer)
    {
        var type = (MessageType)buffer.Span[0];
        var control = (InputType)buffer.Span[1];

        logger.LogTrace("player input {} {} {}", player.Id, type, control);

        var isEnable = type switch
        {
            MessageType.Enable => true,
            MessageType.Disable => false,
            _ => throw new ArgumentException("invalid message type")
        };

        player.LastInput = DateTime.Now;

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
    }
}
