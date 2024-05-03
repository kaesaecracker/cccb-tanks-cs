using System.Buffers;
using System.Net.WebSockets;
using DisplayCommands;
using TanksServer.Graphics;

namespace TanksServer.Interactivity;

internal sealed class ClientScreenServerConnection : WebsocketServerConnection
{
    private sealed record class Package(IMemoryOwner<byte> Pixels, IMemoryOwner<byte>? PlayerData);

    private readonly BufferPool _bufferPool;
    private readonly PlayerScreenData? _playerDataBuilder;
    private readonly Player? _player;
    private int _wantsFrameOnTick = 1;
    private Package? _next;

    public ClientScreenServerConnection(
        WebSocket webSocket,
        ILogger<ClientScreenServerConnection> logger,
        Player? player,
        BufferPool bufferPool
    ) : base(logger, new ByteChannelWebSocket(webSocket, logger, 0))
    {
        _player = player;
        _bufferPool = bufferPool;
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

    public async Task OnGameTickAsync(PixelGrid pixels, GamePixelGrid gamePixelGrid)
    {
        await Task.Yield();

        var nextPixels = _bufferPool.Rent(pixels.Data.Length);
        pixels.Data.CopyTo(nextPixels.Memory);

        IMemoryOwner<byte>? nextPlayerData = null;
        if (_playerDataBuilder != null)
        {
            var data = _playerDataBuilder.Build(gamePixelGrid);
            nextPlayerData = _bufferPool.Rent(data.Length);
            data.CopyTo(nextPlayerData.Memory);
        }

        var next = new Package(nextPixels, nextPlayerData);
        if (Interlocked.Exchange(ref _wantsFrameOnTick, 0) != 0)
        {
            await SendAndDisposeAsync(next);
            return;
        }

        var oldNext = Interlocked.Exchange(ref _next, next);
        oldNext?.Pixels.Dispose();
        oldNext?.PlayerData?.Dispose();
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
            await Socket.SendBinaryAsync(package.Pixels.Memory, package.PlayerData == null);
            if (package.PlayerData != null)
                await Socket.SendBinaryAsync(package.PlayerData.Memory);
        }
        catch (WebSocketException ex)
        {
            Logger.LogWarning(ex, "send failed");
        }
        finally
        {
            package.Pixels.Dispose();
            package.PlayerData?.Dispose();
        }
    }
}
