using System.Buffers;
using System.Net.WebSockets;
using DisplayCommands;
using TanksServer.Graphics;

namespace TanksServer.Interactivity;

internal sealed class ClientScreenServerConnection : WebsocketServerConnection
{
    private sealed record class Package(
        IMemoryOwner<byte> PixelsOwner,
        Memory<byte> Pixels,
        IMemoryOwner<byte>? PlayerDataOwner,
        Memory<byte>? PlayerData
    );

    private readonly MemoryPool<byte> _memoryPool = MemoryPool<byte>.Shared;
    private readonly PlayerScreenData? _playerDataBuilder;
    private readonly Player? _player;
    private int _wantsFrameOnTick = 1;
    private Package? _next;

    public ClientScreenServerConnection(WebSocket webSocket,
        ILogger<ClientScreenServerConnection> logger,
        Player? player) : base(logger, new ByteChannelWebSocket(webSocket, logger, 0))
    {
        _player = player;
        _player?.IncrementConnectionCount();
        _playerDataBuilder = player == null
            ? null
            : new PlayerScreenData(logger, player);
    }

    protected override ValueTask HandleMessageAsync(Memory<byte> _)
    {
        if (_wantsFrameOnTick != 0)
            return ValueTask.CompletedTask;

        var package = Interlocked.Exchange(ref _next, null);
        if (package != null)
            return SendAndDisposeAsync(package);

        // the delay between one exchange and this set could be enough for another frame to complete
        // this would mean the client simply drops a frame, so this should be fine
        _wantsFrameOnTick = 1;
        return ValueTask.CompletedTask;
    }

    public async ValueTask OnGameTickAsync(PixelGrid pixels, GamePixelGrid gamePixelGrid)
    {
        await Task.Yield();

        var nextPixelsOwner = _memoryPool.Rent(pixels.Data.Length);
        var nextPixels = nextPixelsOwner.Memory[..pixels.Data.Length];
        pixels.Data.CopyTo(nextPixels);

        IMemoryOwner<byte>? nextPlayerDataOwner = null;
        Memory<byte>? nextPlayerData = null;
        if (_playerDataBuilder != null)
        {
            var data = _playerDataBuilder.Build(gamePixelGrid);
            nextPlayerDataOwner = _memoryPool.Rent(data.Length);
            nextPlayerData = nextPlayerDataOwner.Memory[..data.Length];
            data.CopyTo(nextPlayerData.Value);
        }

        var next = new Package(nextPixelsOwner, nextPixels, nextPlayerDataOwner, nextPlayerData);
        if (Interlocked.Exchange(ref _wantsFrameOnTick, 0) != 0)
        {
            await SendAndDisposeAsync(next);
            return;
        }

        var oldNext = Interlocked.Exchange(ref _next, next);
        oldNext?.PixelsOwner.Dispose();
        oldNext?.PlayerDataOwner?.Dispose();
    }

    public override ValueTask RemovedAsync()
    {
        _player?.DecrementConnectionCount();
        return ValueTask.CompletedTask;
    }

    private async ValueTask SendAndDisposeAsync(Package package)
    {
        try
        {
            await Socket.SendBinaryAsync(package.Pixels, package.PlayerData == null);
            if (package.PlayerData != null)
                await Socket.SendBinaryAsync(package.PlayerData.Value);
        }
        catch (WebSocketException ex)
        {
            Logger.LogWarning(ex, "send failed");
        }
        finally
        {
            package.PixelsOwner.Dispose();
            package.PlayerDataOwner?.Dispose();
        }
    }
}
