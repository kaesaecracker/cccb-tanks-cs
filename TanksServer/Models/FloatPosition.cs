using TanksServer.GameLogic;

namespace TanksServer.Models;

internal readonly struct FloatPosition(double x, double y)
{
    public double X { get; } = (x + MapService.PixelsPerRow) % MapService.PixelsPerRow;
    public double Y { get; } = (y + MapService.PixelsPerColumn) % MapService.PixelsPerColumn;
}