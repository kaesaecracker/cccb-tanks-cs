using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace TanksServer.GameLogic;

internal sealed class MapService
{
    public const ushort TilesPerRow = 44;
    public const ushort TilesPerColumn = 20;
    public const ushort TileSize = 8;
    public const ushort PixelsPerRow = TilesPerRow * TileSize;
    public const ushort PixelsPerColumn = TilesPerColumn * TileSize;

    private readonly Dictionary<string, bool[,]> _maps = new();

    public IEnumerable<string> MapNames => _maps.Keys;

    public Map Current { get; private set; }

    public MapService()
    {
        foreach (var file in Directory.EnumerateFiles("./assets/maps/", "*.txt"))
            LoadMapString(file);
        foreach (var file in Directory.EnumerateFiles("./assets/maps/", "*.png"))
            LoadMapPng(file);

        var chosenMapIndex = Random.Shared.Next(_maps.Count);
        var chosenMapName = _maps.Keys.Skip(chosenMapIndex).First();
        Current = new Map(chosenMapName, _maps[chosenMapName]);
    }

    private void LoadMapPng(string file)
    {
        using var image = Image.Load<Rgba32>(file);

        if (image.Width != PixelsPerRow || image.Height != PixelsPerColumn)
            throw new FileLoadException($"invalid image size in file {file}");

        var whitePixel = new Rgba32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
        var walls = new bool[PixelsPerRow, PixelsPerColumn];

        for (var y = 0; y < image.Height; y++)
        for (var x = 0; x < image.Width; x++)
            walls[x, y] = image[x, y] == whitePixel;

        _maps.Add(Path.GetFileName(file), walls);
    }

    private void LoadMapString(string file)
    {
        var map = File.ReadAllText(file).ReplaceLineEndings(string.Empty).Trim();
        if (map.Length != TilesPerColumn * TilesPerRow)
            throw new FileLoadException($"cannot load map {file}: invalid length");

        var walls = new bool[PixelsPerRow, PixelsPerColumn];

        for (ushort tileX = 0; tileX < TilesPerRow; tileX++)
        for (ushort tileY = 0; tileY < TilesPerColumn; tileY++)
        {
            var tile = new TilePosition(tileX, tileY);
            if (map[tileX + tileY * TilesPerRow] != '#')
                continue;

            for (byte pixelInTileX = 0; pixelInTileX < TileSize; pixelInTileX++)
            for (byte pixelInTileY = 0; pixelInTileY < TileSize; pixelInTileY++)
            {
                var (x, y) = tile.ToPixelPosition().GetPixelRelative(pixelInTileX, pixelInTileY);
                walls[x, y] = true;
            }
        }

        _maps.Add(Path.GetFileName(file), walls);
    }

    public bool TrySwitchTo(string name)
    {
        if (!_maps.TryGetValue(name, out var mapData))
            return false;
        Current = new Map(name, (bool[,]) mapData.Clone());
        return true;
    }
}

internal sealed class Map(string name, bool[,] walls)
{
    public string Name => name;

    public bool IsWall(int x, int y) => walls[x, y];

    public bool IsWall(PixelPosition position) => walls[position.X, position.Y];

    public bool IsWall(TilePosition position)
    {
        var pixel = position.ToPixelPosition();

        for (short dx = 1; dx < MapService.TilesPerRow - 1; dx++)
        for (short dy = 1; dy < MapService.TilesPerColumn - 1; dy++)
        {
            if (IsWall(pixel.GetPixelRelative(dx, dy)))
                return true;
        }

        return false;
    }

    public void DestroyWallAt(PixelPosition pixel) => walls[pixel.X, pixel.Y] = false;
}
