using System.Buffers;
using System.IO;
using System.Net.WebSockets;
using System.Text.Json;
using TanksServer.GameLogic;

namespace TanksServer.Interactivity;

internal sealed class PlayerInfoConnection
    : DroppablePackageRequestConnection<IMemoryOwner<byte>>
{
    private readonly Player _player;
    private readonly MapEntityManager _entityManager;
    private readonly BufferPool _bufferPool;
    private readonly MemoryStream _tempStream = new();
    private IMemoryOwner<byte>? _lastMessage = null;

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

    public async Task OnGameTickAsync()
    {
        await Task.Yield();

        var response = await GenerateMessageAsync();
        if (response != null)
            SetNextPackage(response);
    }

    public override void Dispose()
    {
        base.Dispose();
        _player.DecrementConnectionCount();
    }

    private async ValueTask<IMemoryOwner<byte>?> GenerateMessageAsync()
    {
        var tank = _entityManager.GetCurrentTankOfPlayer(_player);
        var info = new PlayerInfo(_player, _player.Controls.ToDisplayString(), tank);

        _tempStream.Position = 0;
        await JsonSerializer.SerializeAsync(_tempStream, info, AppSerializerContext.Default.PlayerInfo);

        var messageLength = (int)_tempStream.Position;
        var owner = _bufferPool.Rent(messageLength);


        _tempStream.Position = 0;
        await _tempStream.ReadExactlyAsync(owner.Memory);

        if (_lastMessage == null || !owner.Memory.Span.SequenceEqual(_lastMessage.Memory.Span))
            return owner;

        owner.Dispose();
        return null;
    }

    protected override async ValueTask SendPackageAsync(IMemoryOwner<byte> data)
    {
        await Socket.SendTextAsync(data.Memory);
        Interlocked.Exchange(ref _lastMessage, data)?.Dispose();
    }
}

internal record struct PlayerInfo(
    Player Player,
    string Controls,
    Tank? Tank
);
