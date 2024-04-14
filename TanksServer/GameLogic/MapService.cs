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

    public Map Current { get; }

    public MapService()
    {
        var textMaps = Directory.EnumerateFiles("./assets/maps/", "*.txt").Select(LoadMapString);
        var pngMaps = Directory.EnumerateFiles("./assets/maps/", "*.png").Select(LoadMapPng);

        var maps = textMaps.Concat(pngMaps).ToList();
        Current = maps[Random.Shared.Next(maps.Count)];
    }

    private static Map LoadMapPng(string file)
    {
        var dict = new Dictionary<PixelPosition, bool>();
        using var image = Image.Load<Rgba32>(file);

        if (image.Width != PixelsPerRow || image.Height != PixelsPerColumn)
            throw new FileLoadException($"invalid image size in file {file}");

        var whitePixel = new Rgba32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
        for (var y = 0; y < image.Height; y++)
        for (var x = 0; x < image.Width; x++)
        {
            if (image[x, y] == whitePixel)
                dict[new PixelPosition(x, y)] = true;
        }

        return new Map(dict);
    }

    private static Map LoadMapString(string file)
    {
        var map = File.ReadAllText(file).ReplaceLineEndings(string.Empty).Trim();
        if (map.Length != TilesPerColumn * TilesPerRow)
            throw new FileLoadException($"cannot load map {file}: invalid length");

        var dict = new Dictionary<PixelPosition, bool>();

        for (ushort tileY = 0; tileY < MapService.TilesPerColumn; tileY++)
        for (ushort tileX = 0; tileX < MapService.TilesPerRow; tileX++)
        {
            var tile = new TilePosition(tileX, tileY);
            if (map[tileX + tileY * TilesPerRow] != '#')
                continue;

            for (byte pixelInTileY = 0; pixelInTileY < MapService.TileSize; pixelInTileY++)
            for (byte pixelInTileX = 0; pixelInTileX < MapService.TileSize; pixelInTileX++)
            {
                var pixel = tile.ToPixelPosition().GetPixelRelative(pixelInTileX, pixelInTileY);
                dict[pixel] = true;
            }
        }

        return new Map(dict);
    }
}

internal sealed class Map(IReadOnlyDictionary<PixelPosition, bool> walls)
{
    public bool IsCurrentlyWall(PixelPosition position) => walls.TryGetValue(position, out var value) && value;
}
