using System.Diagnostics;
using TanksServer.GameLogic;
using TanksServer.Graphics;

namespace TanksServer.Interactivity;

internal sealed class PlayerScreenData(ILogger logger, Player player)
{
    private readonly Memory<byte> _data = new byte[MapService.PixelsPerRow * MapService.PixelsPerColumn / 2];
    private int _count;

    public ReadOnlyMemory<byte> Build(GamePixelGrid gamePixelGrid)
    {
        Clear();
        foreach (var gamePixel in gamePixelGrid)
        {
            if (!gamePixel.EntityType.HasValue)
                continue;
            Add(gamePixel.EntityType.Value, gamePixel.BelongsTo == player);
        }

        var index = _count / 2 + (_count % 2 == 0 ? 0 : 1);

        if (logger.IsEnabled(LogLevel.Trace))
            logger.LogTrace("packet length: {} (count={})", index, _count);

        return _data[..index];
    }

    private void Add(GamePixelEntityType entityKind, bool isCurrentPlayer)
    {
        var result = (byte)(isCurrentPlayer ? 0x1 : 0x0);
        var kind = (byte)entityKind;
        Debug.Assert(kind <= 3);
        result += (byte)(kind << 2);

        var index = _count / 2;
        if (_count % 2 != 0)
            _data.Span[index] |= (byte)(result << 4);
        else
            _data.Span[index] = result;
        _count++;
    }

    private void Clear()
    {
        _count = 0;
        _data.Span.Clear();
    }
}
