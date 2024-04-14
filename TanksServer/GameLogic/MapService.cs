using System.IO;

namespace TanksServer.GameLogic;

internal sealed class MapService
{
    public const ushort TilesPerRow = 44;
    public const ushort TilesPerColumn = 20;
    public const ushort TileSize = 8;
    public const ushort PixelsPerRow = TilesPerRow * TileSize;
    public const ushort PixelsPerColumn = TilesPerColumn * TileSize;
    private readonly string _map;
    private readonly ILogger<MapService> _logger;

    private string[] LoadMaps() => Directory.EnumerateFiles("./assets/maps/", "*.txt")
        .Select(LoadMap)
        .Where(s => s != null)
        .Select(s => s!)
        .ToArray();

    private string? LoadMap(string file)
    {
        var text = File.ReadAllText(file).ReplaceLineEndings(string.Empty).Trim();
        if (text.Length != TilesPerColumn * TilesPerRow)
        {
            _logger.LogWarning("cannot load map {}: invalid length", file);
            return null;
        }

        return text;
    }

    public MapService(ILogger<MapService> logger)
    {
        _logger = logger;
        var maps = LoadMaps();
        _map = maps[Random.Shared.Next(0, maps.Length)];
    }

    private char this[int tileX, int tileY] => _map[tileX + tileY * TilesPerRow];

    public bool IsCurrentlyWall(TilePosition position) => this[position.X, position.Y] == '#';
}
