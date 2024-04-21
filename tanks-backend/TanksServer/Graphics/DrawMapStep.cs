using TanksServer.GameLogic;

namespace TanksServer.Graphics;

internal sealed class DrawMapStep(MapService map) : IDrawStep
{
    public void Draw(GamePixelGrid pixels)
    {
        for (ushort y = 0; y < MapService.PixelsPerColumn; y++)
        for (ushort x = 0; x < MapService.PixelsPerRow; x++)
        {
            var pixel = new PixelPosition(x, y);
            if (!map.Current.IsWall(pixel))
                continue;

            pixels[x, y].EntityType = GamePixelEntityType.Wall;
        }
    }
}
