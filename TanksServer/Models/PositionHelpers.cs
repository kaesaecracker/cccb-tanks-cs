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
            x: (ushort)(position.X * MapService.TileSize + subX),
            y: (ushort)(position.Y * MapService.TileSize + subY)
        );
    }

    public static PixelPosition GetPixelRelative(this PixelPosition position, byte subX, byte subY)
    {
        Debug.Assert(subX < 8);
        Debug.Assert(subY < 8);
        return new PixelPosition((ushort)(position.X + subX), (ushort)(position.Y + subY));
    }

    public static PixelPosition ToPixelPosition(this FloatPosition position) => new(
        x: (ushort)((int)position.X % MapService.PixelsPerRow),
        y: (ushort)((int)position.Y % MapService.PixelsPerRow)
    );

    public static TilePosition ToTilePosition(this PixelPosition position) => new(
        x: (ushort)(position.X / MapService.TileSize),
        y: (ushort)(position.Y / MapService.TileSize)
    );
}