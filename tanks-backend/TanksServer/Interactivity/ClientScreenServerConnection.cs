using System.Net.WebSockets;
using DisplayCommands;
using TanksServer.Graphics;

namespace TanksServer.Interactivity;

internal sealed class ClientScreenServerConnection(
    WebSocket webSocket,
    ILogger<ClientScreenServerConnection> logger,
    string? playerName = null
) : WebsocketServerConnection(logger, new ByteChannelWebSocket(webSocket, logger, 0))
{
    private bool _wantsFrameOnTick = true;

    private PixelGrid? _lastSentPixels;
    private PixelGrid? _nextPixels;
    private readonly PlayerScreenData? _nextPlayerData = playerName != null ? new PlayerScreenData(logger) : null;

    protected override async ValueTask HandleMessageLockedAsync(Memory<byte> _)
    {
        if (_nextPixels == null)
        {
            _wantsFrameOnTick = true;
            return;
        }

        await SendNowAsync();
    }

    public ValueTask OnGameTickAsync(PixelGrid pixels, GamePixelGrid gamePixelGrid) => LockedAsync(async () =>
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

        _nextPixels = pixels;
        if (_wantsFrameOnTick)
            _ = await SendNowAsync();
    });

    private async ValueTask<bool> SendNowAsync()
    {
        var pixels = _nextPixels
                     ?? throw new InvalidOperationException("next pixels not set");

        try
        {
            await Socket.SendBinaryAsync(pixels.Data, _nextPlayerData == null);
            if (_nextPlayerData != null)
                await Socket.SendBinaryAsync(_nextPlayerData.GetPacket());

            _lastSentPixels = _nextPixels;
            _nextPixels = null;
            _wantsFrameOnTick = false;
            return true;
        }
        catch (WebSocketException ex)
        {
            Logger.LogWarning(ex, "send failed");
            return false;
        }
    }
}
