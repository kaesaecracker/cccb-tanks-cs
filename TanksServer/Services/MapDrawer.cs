namespace TanksServer.Services;

internal class MapDrawer(MapService map):ITickStep
{
    private const uint GameFieldPixelCount = MapService.PixelsPerRow * MapService.PixelsPerColumn;

    private void DrawInto(DisplayPixelBuffer buf)
    {
        for (var tileY = 0; tileY < MapService.TilesPerColumn; tileY++)
        for (var tileX = 0; tileX < MapService.TilesPerRow; tileX++)
        {
            if (!map.IsCurrentlyWall(tileX, tileY))
                continue;

            var absoluteTilePixelY = tileY * MapService.TileSize;
            for (var pixelInTileY = 0; pixelInTileY < MapService.TileSize; pixelInTileY++)
            {
                var absoluteRowStartPixelIndex = (absoluteTilePixelY + pixelInTileY) * MapService.PixelsPerRow
                                                 + tileX * MapService.TileSize;
                for (var pixelInTileX = 0; pixelInTileX < MapService.TileSize; pixelInTileX++)
                    buf.Pixels[absoluteRowStartPixelIndex + pixelInTileX] = pixelInTileX % 2 == pixelInTileY % 2;
            }
        }
    }

    private DisplayPixelBuffer CreateGameFieldPixelBuffer()
    {
        var data = new byte[10 + GameFieldPixelCount / 8];
        var result = new DisplayPixelBuffer(data)
        {
            Magic1 = 0,
            Magic2 = 19,
            X = 0,
            Y = 0,
            WidthInTiles = MapService.TilesPerRow,
            HeightInPixels = MapService.PixelsPerColumn
        };
        return result;
    }

    private DisplayPixelBuffer? _lastFrame;

    public DisplayPixelBuffer LastFrame
    {
        get => _lastFrame ?? throw new InvalidOperationException("first frame not yet drawn");
        private set => _lastFrame = value;
    }

    public Task TickAsync()
    {
        var buffer = CreateGameFieldPixelBuffer();
        DrawInto(buffer);
        LastFrame = buffer;
        return Task.CompletedTask;
    }
}
