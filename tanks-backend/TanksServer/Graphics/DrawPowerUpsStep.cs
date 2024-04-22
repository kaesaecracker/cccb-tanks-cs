using TanksServer.GameLogic;

namespace TanksServer.Graphics;

internal sealed class DrawPowerUpsStep(MapEntityManager entityManager) : IDrawStep
{
    private readonly Sprite _explosiveSprite = Sprite.FromImageFile("assets/powerup_explosive.png");

    public void Draw(GamePixelGrid pixels)
    {
        foreach (var powerUp in entityManager.PowerUps)
        {
            var position = powerUp.Bounds.TopLeft;

            for (byte dy = 0; dy < MapService.TileSize; dy++)
            for (byte dx = 0; dx < MapService.TileSize; dx++)
            {
                var pixelState = _explosiveSprite[dx, dy];
                if (!pixelState.HasValue)
                    continue;

                var (x, y) = position.GetPixelRelative(dx, dy);
                pixels[x, y].EntityType = pixelState.Value
                    ? GamePixelEntityType.PowerUp
                    : null;
            }
        }
    }
}
