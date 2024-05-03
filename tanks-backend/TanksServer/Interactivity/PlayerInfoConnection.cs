using System.Buffers;
using System.IO;
using System.Net.WebSockets;
using System.Text.Json;
using TanksServer.GameLogic;

namespace TanksServer.Interactivity;

// MemoryStream is IDisposable but does not need to be disposed
#pragma warning disable CA1001
internal sealed class PlayerInfoConnection : WebsocketServerConnection
#pragma warning restore CA1001
{
    private readonly Player _player;
    private readonly MapEntityManager _entityManager;
    private readonly BufferPool _bufferPool;
    private readonly MemoryStream _tempStream = new();
    private int _wantsInfoOnTick = 1;
    private IMemoryOwner<byte>? _lastMessage = null;
    private IMemoryOwner<byte>? _nextMessage = null;

    public PlayerInfoConnection(
        Player player,
        ILogger logger,
        WebSocket rawSocket,
        MapEntityManager entityManager,
        BufferPool bufferPool
    ) : base(logger, new ByteChannelWebSocket(rawSocket, logger, 0))
    {
        _player = player;
        _entityManager = entityManager;
        _bufferPool = bufferPool;
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

    public async Task OnGameTickAsync()
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

    private async ValueTask<IMemoryOwner<byte>> GenerateMessageAsync()
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
        var owner = _bufferPool.Rent(messageLength);

        _tempStream.Position = 0;
        await _tempStream.ReadExactlyAsync(owner.Memory);
        return owner;
    }

    private async ValueTask SendAndDisposeAsync(IMemoryOwner<byte> data)
    {
        await Socket.SendTextAsync(data.Memory);
        Interlocked.Exchange(ref _lastMessage, data)?.Dispose();
    }
}
