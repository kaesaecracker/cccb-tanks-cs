using TanksServer.Models;

namespace TanksServer.Services;

internal sealed class MapService
{
    public const int TilesPerRow = 44;
    public const int TilesPerColumn = 20;
    public const int TileSize = 8;
    public const int PixelsPerRow = TilesPerRow * TileSize;
    public const int PixelsPerColumn = TilesPerColumn * TileSize;

    private readonly string _map =
        """
            ############################################
            #...................##.....................#
            #...................##.....................#
            #.....####......................####.......#
            #..........................................#
            #............###...........###.............#
            #............#...............#.............#
            #...##.......#...............#......##.....#
            #....#..............................#......#
            #....#..##......................##..#......#
            #....#..##......................##..#......#
            #....#..............................#......#
            #...##.......#...............#......##.....#
            #............#...............#.............#
            #............###...........###.............#
            #..........................................#
            #.....####......................####.......#
            #...................##.....................#
            #...................##.....................#
            ############################################
            """
            .ReplaceLineEndings(string.Empty);

    private char this[int tileX, int tileY] => _map[tileX + tileY * TilesPerRow];

    public bool IsCurrentlyWall(TilePosition position)
    {
        return this[position.X, position.Y] == '#';
    }
}
