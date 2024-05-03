using TanksServer.GameLogic;

namespace TanksServer.Models;

internal static class PositionHelpers
{
    public static PixelPosition GetPixelRelative(this PixelPosition position, short subX, short subY)
        => new(position.X + subX, position.Y + subY);

    public static PixelPosition ToPixelPosition(this FloatPosition position)
        => new((int)Math.Round(position.X), (int)Math.Round(position.Y));

    public static PixelPosition ToPixelPosition(this TilePosition position) => new(
        (ushort)(position.X * MapService.TileSize),
        (ushort)(position.Y * MapService.TileSize)
    );

    public static TilePosition ToTilePosition(this PixelPosition position) => new(
        (ushort)(position.X / MapService.TileSize),
        (ushort)(position.Y / MapService.TileSize)
    );

    public static FloatPosition ToFloatPosition(this PixelPosition position) => new(position.X, position.Y);

    public static double Distance(this FloatPosition p1, FloatPosition p2)
        => Math.Sqrt(
            Math.Pow(p1.X - p2.X, 2) +
            Math.Pow(p1.Y - p2.Y, 2)
        );

    public static PixelBounds GetBoundsForCenter(this FloatPosition position, ushort size)
    {
        var sub = (short)(-size / 2d);
        var add = (short)(size / 2d - 1);
        var pixelPosition = position.ToPixelPosition();
        return new PixelBounds(
            pixelPosition.GetPixelRelative(sub, sub),
            pixelPosition.GetPixelRelative(add, add)
        );
    }
}
