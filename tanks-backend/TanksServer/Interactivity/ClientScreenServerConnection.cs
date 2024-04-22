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
            Logger.LogTrace("client does not want a frame yet");
            return;
        }

        _nextFrameAfter = DateTime.Today + minFrameTime;

        if (_playerScreenData != null)
            RefreshPlayerSpecificData(gamePixelGrid);

        Logger.LogTrace("sending");
        try
        {
            Logger.LogTrace("sending {} bytes of pixel data", pixels.Data.Length);
            await Socket.SendBinaryAsync(pixels.Data, _playerScreenData == null);
            if (_playerScreenData != null)
                await Socket.SendBinaryAsync(_playerScreenData.GetPacket());
        }
        catch (WebSocketException ex)
        {
            Logger.LogWarning(ex, "send failed");
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

    protected override ValueTask HandleMessageAsync(Memory<byte> _)
    {
        _wantedFrames.Release();
        return ValueTask.CompletedTask;
    }
}
