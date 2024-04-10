using System.Diagnostics;
using TanksServer.GameLogic;

namespace TanksServer.Models;

internal static class PositionHelpers
{
    public static PixelPosition GetPixelRelative(this TilePosition position, byte subX, byte subY)
    {
        Debug.Assert(subX < 8);
        Debug.Assert(subY < 8);
        return new PixelPosition(
            X: position.X * MapService.TileSize + subX,
            Y: position.Y * MapService.TileSize + subY
        );
    }


    public static PixelPosition ToPixelPosition(this FloatPosition position) => new(
        X: (int)position.X % MapService.PixelsPerRow,
        Y: (int)position.Y % MapService.PixelsPerRow
    );

    public static TilePosition ToTilePosition(this PixelPosition position) => new(
        X: position.X / MapService.TileSize,
        Y: position.Y / MapService.TileSize
    );
}