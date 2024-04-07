using TanksServer.Services;

namespace TanksServer.Models;

internal readonly record struct FloatPosition(double X, double Y)
{
    public PixelPosition ToPixelPosition() => new((int)X % MapService.PixelsPerRow, (int)Y % MapService.PixelsPerRow);
}
