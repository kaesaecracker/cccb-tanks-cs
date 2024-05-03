using System.Buffers;
using System.IO;
using System.Net.WebSockets;
using System.Text.Json;
using TanksServer.GameLogic;

namespace TanksServer.Interactivity;

internal sealed class PlayerInfoConnection : WebsocketServerConnection
{
    private readonly Player _player;
    private readonly MapEntityManager _entityManager;
    private readonly MemoryStream _tempStream = new();
    private readonly MemoryPool<byte> _memoryPool = MemoryPool<byte>.Shared;
    private int _wantsInfoOnTick = 1;
    private Package? _lastMessage = null;
    private Package? _nextMessage = null;

    private sealed record class Package(IMemoryOwner<byte> Owner, Memory<byte> Memory);

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

        var response = await GenerateMessageAsync();
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

    private async ValueTask<Package> GenerateMessageAsync()
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

        _tempStream.Position = 0;
        await JsonSerializer.SerializeAsync(_tempStream, info, AppSerializerContext.Default.PlayerInfo);

        var messageLength = (int)_tempStream.Position;
        var owner = _memoryPool.Rent(messageLength);
        var package = new Package(owner, owner.Memory[..messageLength]);

        _tempStream.Position = 0;
        await _tempStream.ReadExactlyAsync(package.Memory);
        return package;
    }

    private async ValueTask SendAndDisposeAsync(Package data)
    {
        await Socket.SendTextAsync(data.Memory);
        Interlocked.Exchange(ref _lastMessage, data)?.Owner.Dispose();
    }
}
