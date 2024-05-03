using TanksServer.GameLogic;

namespace TanksServer.Graphics;

internal sealed class DrawPowerUpsStep(MapEntityManager entityManager) : IDrawStep
{
    private readonly Sprite _genericSprite = Sprite.FromImageFile("assets/powerup_explosive.png");
    private readonly Sprite _smartSprite = Sprite.FromImageFile("assets/powerup_smart.png");

    public void Draw(GamePixelGrid pixels)
    {
        foreach (var powerUp in entityManager.PowerUps)
        {
            var sprite = _genericSprite;
            if (powerUp is { Type: PowerUpType.MagazineType, MagazineType: MagazineType.Smart })
                sprite = _smartSprite;

            DrawPowerUp(pixels, sprite, powerUp.Bounds.TopLeft);
        }
    }

    private static void DrawPowerUp(GamePixelGrid pixels, Sprite sprite, PixelPosition position)
    {
        for (byte dy = 0; dy < MapService.TileSize; dy++)
        for (byte dx = 0; dx < MapService.TileSize; dx++)
        {
            var pixelState = sprite[dx, dy];
            if (!pixelState.HasValue)
                continue;

            var (x, y) = position.GetPixelRelative(dx, dy);
            pixels[x, y].EntityType = pixelState.Value
                ? GamePixelEntityType.PowerUp
                : null;
        }
    }
}
