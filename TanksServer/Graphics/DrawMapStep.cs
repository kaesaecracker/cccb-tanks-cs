using DisplayCommands;
using TanksServer.GameLogic;

namespace TanksServer.Graphics;

internal sealed class DrawMapStep(MapService map) : IDrawStep
{
    public void Draw(PixelGrid buffer)
    {
        for (ushort tileY = 0; tileY < MapService.TilesPerColumn; tileY++)
        for (ushort tileX = 0; tileX < MapService.TilesPerRow; tileX++)
        {
            var tile = new TilePosition(tileX, tileY);
            if (!map.IsCurrentlyWall(tile))
                continue;

            for (byte pixelInTileY = 0; pixelInTileY < MapService.TileSize; pixelInTileY++)
            for (byte pixelInTileX = 0; pixelInTileX < MapService.TileSize; pixelInTileX++)
            {
                var (x, y) = tile.ToPixelPosition().GetPixelRelative(pixelInTileX, pixelInTileY);
                buffer[(ushort)x, (ushort)y] = pixelInTileX % 2 == pixelInTileY % 2;
            }
        }
    }
}
