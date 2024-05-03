namespace TanksServer.GameLogic;

internal sealed class Map(string name, bool[,] walls)
{
    public string Name => name;

    public bool IsWall(int x, int y) => walls[x, y];

    public bool IsWall(PixelPosition position) => walls[position.X, position.Y];

    public bool IsWall(TilePosition position)
    {
        var pixel = position.ToPixelPosition();

        for (short dx = 0; dx < MapService.TileSize; dx++)
        for (short dy = 0; dy < MapService.TileSize; dy++)
        {
            if (IsWall(pixel.GetPixelRelative(dx, dy)))
                return true;
        }

        return false;
    }

    public bool TryDestroyWallAt(PixelPosition pixel)
    {
        var result = walls[pixel.X, pixel.Y];
        if (result)
            walls[pixel.X, pixel.Y] = false;
        return result;
    }
}
