using DisplayCommands;
using TanksServer.GameLogic;

namespace TanksServer.Graphics;

internal sealed class MapDrawer(MapService map) : IDrawStep
{
    public void Draw(PixelGrid buffer)
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
                var position = tile.GetPixelRelative(pixelInTileX, pixelInTileY);
                buffer[position.X, position.Y] = pixelInTileX % 2 == pixelInTileY % 2;
            }
        }
    }
}