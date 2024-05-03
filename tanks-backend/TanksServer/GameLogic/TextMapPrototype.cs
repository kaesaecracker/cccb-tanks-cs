namespace TanksServer.GameLogic;

internal sealed class TextMapPrototype : MapPrototype
{
    private readonly string _name;
    private readonly string _text;

    public TextMapPrototype(string name, string text)
    {
        if (text.Length != MapService.TilesPerColumn * MapService.TilesPerRow)
            throw new ArgumentException($"cannot load map {name}: invalid length");
        _name = name;
        _text = text;
    }

    public override Map CreateInstance()
    {
        var walls = new bool[MapService.PixelsPerRow, MapService.PixelsPerColumn];

        for (ushort tileX = 0; tileX < MapService.TilesPerRow; tileX++)
        for (ushort tileY = 0; tileY < MapService.TilesPerColumn; tileY++)
        {
            var tile = new TilePosition(tileX, tileY);
            if (_text[tileX + tileY * MapService.TilesPerRow] != '#')
                continue;

            for (byte pixelInTileX = 0; pixelInTileX < MapService.TileSize; pixelInTileX++)
            for (byte pixelInTileY = 0; pixelInTileY < MapService.TileSize; pixelInTileY++)
            {
                var (x, y) = tile.ToPixelPosition().GetPixelRelative(pixelInTileX, pixelInTileY);
                walls[x, y] = true;
            }
        }

        return new Map(_name, walls);
    }
}
