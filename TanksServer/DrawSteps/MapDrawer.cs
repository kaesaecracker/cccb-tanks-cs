using TanksServer.Helpers;
using TanksServer.Models;
using TanksServer.Services;

namespace TanksServer.DrawSteps;

internal sealed class MapDrawer(MapService map) : IDrawStep
{
    public void Draw(DisplayPixelBuffer buffer)
    {
        for (var tileY = 0; tileY < MapService.TilesPerColumn; tileY++)
        for (var tileX = 0; tileX < MapService.TilesPerRow; tileX++)
        {
            var tile = new TilePosition(tileX, tileY);
            if (!map.IsCurrentlyWall(tile))
                continue;

            for (byte pixelInTileY = 0; pixelInTileY < MapService.TileSize; pixelInTileY++)
            for (byte pixelInTileX = 0; pixelInTileX < MapService.TileSize; pixelInTileX++)
            {
                var index = tile.GetPixelRelative(pixelInTileX, pixelInTileY).ToPixelIndex();
                buffer.Pixels[index] = pixelInTileX % 2 == pixelInTileY % 2;
            }
        }
    }
}
