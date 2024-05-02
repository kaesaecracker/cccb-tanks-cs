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
    private int _wantsInfoOnTick = 1;
    private byte[]? _lastMessage = null;
    private byte[]? _nextMessage = null;

    protected override ValueTask HandleMessageAsync(Memory<byte> buffer)
    {
        var next = Interlocked.Exchange(ref _nextMessage, null);
        if (next != null)
            return SendAndDisposeAsync(next);

        _wantsInfoOnTick = 1;
        return ValueTask.CompletedTask;
    }

    public async ValueTask OnGameTickAsync()
    {
        await Task.Yield();

        var response = GetMessageToSend();
        var wantsNow = Interlocked.Exchange(ref _wantsInfoOnTick, 0) != 0;

        if (wantsNow)
        {
            await SendAndDisposeAsync(response);
            return;
        }

        Interlocked.Exchange(ref _nextMessage, response);
    }

    private byte[] GetMessageToSend()
    {
        var tank = entityManager.GetCurrentTankOfPlayer(player);

        TankInfo? tankInfo = null;
        if (tank != null)
        {
            var magazine = tank.ReloadingUntil > DateTime.Now ? "[ RELOADING ]" : tank.Magazine.ToDisplayString();
            tankInfo = new TankInfo(tank.Orientation, magazine, tank.Position.ToPixelPosition(), tank.Moving);
        }

        var info = new PlayerInfo(player.Name, player.Scores, player.Controls.ToDisplayString(), tankInfo);

        // TODO: switch to async version with pre-allocated buffer / IMemoryOwner
        return JsonSerializer.SerializeToUtf8Bytes(info, AppSerializerContext.Default.PlayerInfo);
    }

    private async ValueTask SendAndDisposeAsync(byte[] data)
    {
        await Socket.SendTextAsync(data);
        Interlocked.Exchange(ref _lastMessage, data);
    }
}
