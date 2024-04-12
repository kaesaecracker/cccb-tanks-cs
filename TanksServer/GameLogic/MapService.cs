namespace TanksServer.GameLogic;

internal sealed class MapService
{
    public const ushort TilesPerRow = 44;
    public const ushort TilesPerColumn = 20;
    public const ushort TileSize = 8;
    public const ushort PixelsPerRow = TilesPerRow * TileSize;
    public const ushort PixelsPerColumn = TilesPerColumn * TileSize;

    private readonly string _map =
        """
            #############..###################.#########
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
            #############..###################.#########
            """
            .ReplaceLineEndings(string.Empty);

    private char this[int tileX, int tileY] => _map[tileX + tileY * TilesPerRow];

    public bool IsCurrentlyWall(TilePosition position)
    {
        return this[position.X, position.Y] == '#';
    }
}
