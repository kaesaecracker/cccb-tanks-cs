using System.Net.WebSockets;
using System.Text.Json;
using TanksServer.GameLogic;

namespace TanksServer.Interactivity;

internal sealed class PlayerInfoConnection : WebsocketServerConnection
{
    private int _wantsInfoOnTick = 1;
    private byte[]? _lastMessage = null;
    private byte[]? _nextMessage = null;
    private readonly Player _player;
    private readonly MapEntityManager _entityManager;

    public PlayerInfoConnection(Player player,
        ILogger logger,
        WebSocket rawSocket,
        MapEntityManager entityManager) : base(logger, new ByteChannelWebSocket(rawSocket, logger, 0))
    {
        _player = player;
        _entityManager = entityManager;
        _player.IncrementConnectionCount();
    }

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

    public override ValueTask RemovedAsync()
    {
        _player.DecrementConnectionCount();
        return ValueTask.CompletedTask;
    }

    private byte[] GetMessageToSend()
    {
        var tank = _entityManager.GetCurrentTankOfPlayer(_player);

        TankInfo? tankInfo = null;
        if (tank != null)
        {
            var magazine = tank.ReloadingUntil > DateTime.Now ? "[ RELOADING ]" : tank.Magazine.ToDisplayString();
            tankInfo = new TankInfo(tank.Orientation, magazine, tank.Position.ToPixelPosition(), tank.Moving);
        }

        var info = new PlayerInfo(
            _player.Name,
            _player.Scores,
            _player.Controls.ToDisplayString(),
            tankInfo,
            _player.OpenConnections);

        // TODO: switch to async version with pre-allocated buffer / IMemoryOwner
        return JsonSerializer.SerializeToUtf8Bytes(info, AppSerializerContext.Default.PlayerInfo);
    }

    private async ValueTask SendAndDisposeAsync(byte[] data)
    {
        await Socket.SendTextAsync(data);
        Interlocked.Exchange(ref _lastMessage, data);
    }
}
