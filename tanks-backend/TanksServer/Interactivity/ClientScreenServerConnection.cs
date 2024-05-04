using System.Buffers;
using System.Diagnostics;
using System.Net.WebSockets;
using DisplayCommands;
using DotNext.Threading;
using TanksServer.Graphics;

namespace TanksServer.Interactivity;

internal sealed class ClientScreenServerConnection
    : WebsocketServerConnection, IDisposable
{
    private readonly BufferPool _bufferPool;
    private readonly PlayerScreenData? _playerDataBuilder;
    private readonly Player? _player;
    private readonly AsyncAutoResetEvent _nextPackageEvent = new(false, 1);
    private int _runningMessageHandlers = 0;
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
        if (Interlocked.Increment(ref _runningMessageHandlers) == 1)
            return Core();

        Interlocked.Decrement(ref _runningMessageHandlers);
        return ValueTask.CompletedTask;

        async ValueTask Core()
        {
            await _nextPackageEvent.WaitAsync();
            var package = Interlocked.Exchange(ref _next, null);
            if (package == null)
                throw new UnreachableException("package should be set here");
            await SendAndDisposeAsync(package);
            Interlocked.Decrement(ref _runningMessageHandlers);
        }
    }

    public async Task OnGameTickAsync(PixelGrid pixels, GamePixelGrid gamePixelGrid)
    {
        await Task.Yield();

        var next = BuildNextPackage(pixels, gamePixelGrid);
        var oldNext = Interlocked.Exchange(ref _next, next);

        _nextPackageEvent.Set();

        oldNext?.Dispose();
    }

    public override ValueTask RemovedAsync()
    {
        _player?.DecrementConnectionCount();
        return ValueTask.CompletedTask;
    }

    public void Dispose()
    {
        _nextPackageEvent.Dispose();
        Interlocked.Exchange(ref _next, null)?.Dispose();
    }

    private Package BuildNextPackage(PixelGrid pixels, GamePixelGrid gamePixelGrid)
    {
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
        return next;
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
            package.Dispose();
        }
    }

    private sealed record class Package(
        IMemoryOwner<byte> Pixels,
        IMemoryOwner<byte>? PlayerData
    ) : IDisposable
    {
        public void Dispose()
        {
            Pixels.Dispose();
            PlayerData?.Dispose();
        }
    }
}
