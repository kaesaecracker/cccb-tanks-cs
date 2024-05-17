using ServicePoint2;
using TanksServer.GameLogic;

namespace TanksServer.Graphics;

internal sealed class DrawMapStep(MapService map) : IDrawStep
{
    public void Draw(GamePixelGrid pixels) => Draw(pixels, map.Current);

    private static void Draw(GamePixelGrid pixels, Map map)
    {
        for (ushort y = 0; y < MapService.PixelsPerColumn; y++)
        for (ushort x = 0; x < MapService.PixelsPerRow; x++)
        {
            if (!map.IsWall(x, y))
                continue;

            pixels[x, y].EntityType = GamePixelEntityType.Wall;
        }
    }

    public static void Draw(PixelGrid pixels, Map map)
    {
        for (ushort y = 0; y < MapService.PixelsPerColumn; y++)
        for (ushort x = 0; x < MapService.PixelsPerRow; x++)
        {
            if (!map.IsWall(x, y))
                continue;
            pixels[x, y] = true;
        }
    }
}
