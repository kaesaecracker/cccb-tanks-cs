using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using TanksServer.GameLogic;

namespace TanksServer.Interactivity;

internal sealed class PlayerInfoConnection(
    Player player,
    ILogger logger,
    WebSocket rawSocket,
    MapEntityManager entityManager
) : WebsocketServerConnection(logger, new ByteChannelWebSocket(rawSocket, logger, 0))
{
    private readonly AppSerializerContext _context = new(new JsonSerializerOptions(JsonSerializerDefaults.Web));
    private bool _wantsInfoOnTick = true;
    private byte[] _lastMessage = [];

    protected override ValueTask HandleMessageLockedAsync(Memory<byte> buffer)
    {
        var response = GetMessageToSend();
        if (response == null)
        {
            Logger.LogTrace("cannot respond directly, increasing wanted frames");
            _wantsInfoOnTick = true;
            return ValueTask.CompletedTask;
        }

        Logger.LogTrace("responding directly");
        return Socket.SendTextAsync(response);
    }

    public ValueTask OnGameTickAsync() => LockedAsync(() =>
    {
        if (!_wantsInfoOnTick)
            return ValueTask.CompletedTask;

        var response = GetMessageToSend();
        if (response == null)
            return ValueTask.CompletedTask;

        Logger.LogTrace("responding indirectly");
        return Socket.SendTextAsync(response);
    });

    private byte[]? GetMessageToSend()
    {
        var tank = entityManager.GetCurrentTankOfPlayer(player);
        var tankInfo = tank != null
            ? new TankInfo(tank.Orientation, tank.ExplosiveBullets, tank.Position.ToPixelPosition(), tank.Moving)
            : null;
        var info = new PlayerInfo(player.Name, player.Scores, ControlsToString(player.Controls), tankInfo);
        var response = JsonSerializer.SerializeToUtf8Bytes(info, _context.PlayerInfo);

        if (response.SequenceEqual(_lastMessage))
            return null;

        return _lastMessage = response;
    }

    private static string ControlsToString(PlayerControls controls)
    {
        var str = new StringBuilder("[ ");
        if (controls.Forward)
            str.Append("▲ ");
        if (controls.Backward)
            str.Append("▼ ");
        if (controls.TurnLeft)
            str.Append("◄ ");
        if (controls.TurnRight)
            str.Append("► ");
        if (controls.Shoot)
            str.Append("• ");
        str.Append(']');
        return str.ToString();
    }
}
