using System.Net.WebSockets;
using DisplayCommands;
using TanksServer.Graphics;

namespace TanksServer.Interactivity;

internal sealed class ClientScreenServerConnection(
    WebSocket webSocket,
    ILogger<ClientScreenServerConnection> logger,
    string? playerName = null
) : WebsocketServerConnection(logger, new ByteChannelWebSocket(webSocket, logger, 0)),
    IDisposable
{
    private readonly SemaphoreSlim _wantedFramesOnTick = new(0, 2);
    private readonly SemaphoreSlim _mutex = new(1);

    private PixelGrid? _lastSentPixels = null;
    private PixelGrid? _nextPixels = null;
    private readonly PlayerScreenData? _nextPlayerData = playerName != null ? new PlayerScreenData(logger) : null;

    protected override async ValueTask HandleMessageAsync(Memory<byte> _)
    {
        await _mutex.WaitAsync();
        try
        {
            if (_nextPixels == null)
            {
                _wantedFramesOnTick.Release();
                return;
            }

            _lastSentPixels = _nextPixels;
            _nextPixels = null;
            await SendNowAsync(_lastSentPixels);
        }
        catch (SemaphoreFullException)
        {
            logger.LogWarning("ignoring request for more frames");
        }
        finally
        {
            _mutex.Release();
        }
    }

    public async ValueTask OnGameTickAsync(PixelGrid pixels, GamePixelGrid gamePixelGrid)
    {
        await _mutex.WaitAsync();
        try
        {
            if (pixels == _lastSentPixels)
                return;

            if (_nextPlayerData != null)
            {
                _nextPlayerData.Clear();
                foreach (var gamePixel in gamePixelGrid)
                {
                    if (!gamePixel.EntityType.HasValue)
                        continue;
                    _nextPlayerData.Add(gamePixel.EntityType.Value, gamePixel.BelongsTo?.Name == playerName);
                }
            }

            var sendImmediately = await _wantedFramesOnTick.WaitAsync(TimeSpan.Zero);
            if (sendImmediately)
            {
                await SendNowAsync(pixels);
                return;
            }

            _wantedFramesOnTick.Release();
            _nextPixels = pixels;
        }
        finally
        {
            _mutex.Release();
        }
    }

    private async ValueTask SendNowAsync(PixelGrid pixels)
    {
        Logger.LogTrace("sending");
        try
        {
            Logger.LogTrace("sending {} bytes of pixel data", pixels.Data.Length);
            await Socket.SendBinaryAsync(pixels.Data, _nextPlayerData == null);
            if (_nextPlayerData != null)
            {
                await Socket.SendBinaryAsync(_nextPlayerData.GetPacket());
            }
        }
        catch (WebSocketException ex)
        {
            Logger.LogWarning(ex, "send failed");
        }
    }

    public void Dispose() => _wantedFramesOnTick.Dispose();
}
