using System.Diagnostics.CodeAnalysis;
using System.IO;
using TanksServer.Graphics;

namespace TanksServer.GameLogic;

internal abstract class MapPrototype
{
    public abstract Map CreateInstance();
}

internal sealed class MapService
{
    public const ushort TilesPerRow = 44;
    public const ushort TilesPerColumn = 20;
    public const ushort TileSize = 8;
    public const ushort PixelsPerRow = TilesPerRow * TileSize;
    public const ushort PixelsPerColumn = TilesPerColumn * TileSize;

    private readonly Dictionary<string, MapPrototype> _maps = new();

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
        Current = _maps[chosenMapName].CreateInstance();
    }

    public bool TryGetMapByName(string name, [MaybeNullWhen(false)] out MapPrototype map)
        => _maps.TryGetValue(name, out map);

    public void SwitchTo(MapPrototype prototype) => Current = prototype.CreateInstance();

    private void LoadMapPng(string file)
    {
        var name = Path.GetFileName(file);
        var prototype = new SpriteMapPrototype(name, Sprite.FromImageFile(file));
        _maps.Add(Path.GetFileName(file), prototype);
    }

    private void LoadMapString(string file)
    {
        var map = File.ReadAllText(file).ReplaceLineEndings(string.Empty).Trim();
        var name = Path.GetFileName(file);
        var prototype = new TextMapPrototype(name, map);
        _maps.Add(name, prototype);
    }
}
