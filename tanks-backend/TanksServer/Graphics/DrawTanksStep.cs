using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using TanksServer.GameLogic;

namespace TanksServer.Graphics;

internal sealed class DrawTanksStep(MapEntityManager entityManager) : IDrawStep
{
    private readonly bool[,] _tankSprite = LoadTankSprite();

    public void Draw(GamePixelGrid pixels)
    {
        foreach (var tank in entityManager.Tanks)
        {
            var tankPosition = tank.Bounds.TopLeft;

            for (byte dy = 0; dy < MapService.TileSize; dy++)
            for (byte dx = 0; dx < MapService.TileSize; dx++)
            {
                if (!TankSpriteAt(dx, dy, tank.Orientation))
                    continue;

                var (x, y) = tankPosition.GetPixelRelative(dx, dy);
                pixels[x, y].EntityType = GamePixelEntityType.Tank;
                pixels[x, y].BelongsTo = tank.Owner;
            }
        }
    }

    private bool TankSpriteAt(int dx, int dy, int tankRotation)
    {
        var x = tankRotation % 4 * (MapService.TileSize + 1);
        var y = (int)Math.Floor(tankRotation / 4d) * (MapService.TileSize + 1);

        return _tankSprite[x + dx, y + dy];
    }

    private static bool[,] LoadTankSprite()
    {
        using var tankImage = Image.Load<Rgba32>("assets/tank.png");
        var tankSprite = new bool[tankImage.Width, tankImage.Height];

        var whitePixel = new Rgba32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

        for (var y = 0; y < tankImage.Height; y++)
        for (var x = 0; x < tankImage.Width; x++)
            tankSprite[x, y] = tankImage[x, y] == whitePixel;

        return tankSprite;
    }
}
