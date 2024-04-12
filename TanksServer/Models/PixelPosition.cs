using TanksServer.GameLogic;

namespace TanksServer.Models;

internal readonly struct PixelPosition(ushort x, ushort y)
{
    public ushort X { get; } = (ushort)((x + MapService.PixelsPerRow) % MapService.PixelsPerRow);
    public ushort Y { get; } = (ushort)((y + MapService.PixelsPerColumn) % MapService.PixelsPerColumn);
}