using System.Net.WebSockets;
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

        TankInfo? tankInfo = null;
        if (tank != null)
        {
            var magazine = tank.ReloadingUntil > DateTime.Now ? "[ RELOADING ]" : tank.Magazine.ToDisplayString();
            tankInfo = new TankInfo(tank.Orientation, magazine, tank.Position.ToPixelPosition(), tank.Moving);
        }

        var info = new PlayerInfo(player.Name, player.Scores, player.Controls.ToDisplayString(), tankInfo);
        var response = JsonSerializer.SerializeToUtf8Bytes(info, AppSerializerContext.Default.PlayerInfo);

        if (response.SequenceEqual(_lastMessage))
            return null;

        return _lastMessage = response;
    }
}
