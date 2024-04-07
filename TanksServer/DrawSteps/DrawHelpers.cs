using System.Diagnostics;
using TanksServer.Models;
using TanksServer.Services;

namespace TanksServer.DrawSteps;

internal static class DrawHelpers
{
    public static int GetPixelIndex(this PixelPosition position)
    {
        return position.Y * MapService.PixelsPerRow + position.X;
    }

    public static PixelPosition GetPixel(this TilePosition position, byte subX, byte subY)
    {
        Debug.Assert(subX < 8);
        Debug.Assert(subY < 8);
        return new PixelPosition(
            X: position.X * MapService.TileSize + subX,
            Y: position.Y * MapService.TileSize + subY
        );
    }
}
