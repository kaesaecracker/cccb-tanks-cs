using TanksServer.GameLogic;

namespace TanksServer.Graphics;

internal sealed class DrawBulletsStep(MapEntityManager entityManager) : IDrawStep
{
    public void Draw(GamePixelGrid pixels)
    {
        foreach (var bullet in entityManager.Bullets)
        {
            var position = bullet.Position.ToPixelPosition();
            pixels[position.X, position.Y].EntityType = GamePixelEntityType.Bullet;
            pixels[position.X, position.Y].BelongsTo = bullet.Owner;
        }
    }
}
