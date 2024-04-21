using System.Diagnostics;
using System.Net.WebSockets;
using DisplayCommands;
using TanksServer.Graphics;

namespace TanksServer.Interactivity;

internal sealed class ClientScreenServerConnection(
    WebSocket webSocket,
    ILogger<ClientScreenServerConnection> logger,
    TimeSpan minFrameTime,
    Guid? playerGuid = null
) : WebsocketServerConnection(logger, new ByteChannelWebSocket(webSocket, logger, 0)),
    IDisposable
{
    private readonly SemaphoreSlim _wantedFrames = new(1);
    private readonly PlayerScreenData? _playerScreenData = playerGuid.HasValue ? new PlayerScreenData(logger) : null;
    private DateTime _nextFrameAfter = DateTime.Now;

    public void Dispose() => _wantedFrames.Dispose();

    public async Task SendAsync(PixelGrid pixels, GamePixelGrid gamePixelGrid)
    {
        if (_nextFrameAfter > DateTime.Now)
            return;

        if (!await _wantedFrames.WaitAsync(TimeSpan.Zero))
        {
            logger.LogTrace("client does not want a frame yet");
            return;
        }

        _nextFrameAfter = DateTime.Today + minFrameTime;

        if (_playerScreenData != null)
            RefreshPlayerSpecificData(gamePixelGrid);

        logger.LogTrace("sending");
        try
        {
            logger.LogTrace("sending {} bytes of pixel data", pixels.Data.Length);
            await Socket.SendAsync(pixels.Data, _playerScreenData == null);
            if (_playerScreenData != null)
                await Socket.SendAsync(_playerScreenData.GetPacket());
        }
        catch (WebSocketException ex)
        {
            logger.LogWarning(ex, "send failed");
        }
    }

    private void RefreshPlayerSpecificData(GamePixelGrid gamePixelGrid)
    {
        Debug.Assert(_playerScreenData != null);
        _playerScreenData.Clear();
        foreach (var gamePixel in gamePixelGrid)
        {
            if (!gamePixel.EntityType.HasValue)
                continue;
            _playerScreenData.Add(gamePixel.EntityType.Value, gamePixel.BelongsTo?.Id == playerGuid);
        }
    }

    protected override void HandleMessage(Memory<byte> _) => _wantedFrames.Release();
}
