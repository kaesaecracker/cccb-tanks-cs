using TanksServer.GameLogic;

namespace TanksServer.Graphics;

internal sealed class DrawPowerUpsStep(MapEntityManager entityManager) : IDrawStep
{
    private readonly Sprite _genericSprite = Sprite.FromImageFile("assets/powerup_generic.png");
    private readonly Sprite _smartSprite = Sprite.FromImageFile("assets/powerup_smart.png");
    private readonly Sprite _magazineSprite = Sprite.FromImageFile("assets/powerup_magazine.png");
    private readonly Sprite _explosiveSprite = Sprite.FromImageFile("assets/powerup_explosive.png");
    private readonly Sprite _fastSprite = Sprite.FromImageFile("assets/powerup_fastbullet.png");

    public void Draw(GamePixelGrid pixels)
    {
        foreach (var powerUp in entityManager.PowerUps)
        {
            var sprite = powerUp switch
            {
                { Type: PowerUpType.MagazineSize } => _magazineSprite,
                { Type: PowerUpType.MagazineType, MagazineType: MagazineType.Smart } => _smartSprite,
                { Type: PowerUpType.MagazineType, MagazineType: MagazineType.Explosive } => _explosiveSprite,
                { Type: PowerUpType.MagazineType, MagazineType: MagazineType.Fast } => _fastSprite,
                _ => _genericSprite
            };

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
