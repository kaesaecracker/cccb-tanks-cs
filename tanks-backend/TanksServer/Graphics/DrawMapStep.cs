using TanksServer.GameLogic;

namespace TanksServer.Graphics;

internal sealed class DrawMapStep(MapService map) : IDrawStep
{
    public void Draw(GamePixelGrid pixels)
    {
        for (ushort y = 0; y < MapService.PixelsPerColumn; y++)
        for (ushort x = 0; x < MapService.PixelsPerRow; x++)
        {
            if (!map.Current.IsWall(x, y))
                continue;

            pixels[x, y].EntityType = GamePixelEntityType.Wall;
        }
    }
}
