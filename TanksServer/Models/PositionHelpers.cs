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
            (ushort)(position.X * MapService.TileSize + subX),
            (ushort)(position.Y * MapService.TileSize + subY)
        );
    }

    public static PixelPosition GetPixelRelative(this PixelPosition position, byte subX, byte subY)
    {
        Debug.Assert(subX < 8);
        Debug.Assert(subY < 8);
        return new PixelPosition((ushort)(position.X + subX), (ushort)(position.Y + subY));
    }

    public static PixelPosition ToPixelPosition(this FloatPosition position) => new(
        (ushort)((int)position.X % MapService.PixelsPerRow),
        (ushort)((int)position.Y % MapService.PixelsPerRow)
    );

    public static PixelPosition ToPixelPosition(this TilePosition position) => new(
        (ushort)(position.X * MapService.TileSize),
        (ushort)(position.Y * MapService.TileSize)
    );

    public static TilePosition ToTilePosition(this PixelPosition position) => new(
        (ushort)(position.X / MapService.TileSize),
        (ushort)(position.Y / MapService.TileSize)
    );

    public static FloatPosition ToFloatPosition(this PixelPosition position) => new(position.X, position.Y);


    public static double Distance(this FloatPosition p1, FloatPosition p2) =>
        Math.Sqrt(
            Math.Pow(p1.X - p2.X, 2) +
            Math.Pow(p1.Y - p2.Y, 2)
        );
}
