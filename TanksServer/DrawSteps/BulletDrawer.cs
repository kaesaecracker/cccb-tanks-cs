using TanksServer.Helpers;

namespace TanksServer.DrawSteps;

internal sealed class BulletDrawer(BulletManager bullets): IDrawStep
{
    public void Draw(DisplayPixelBuffer buffer)
    {
        foreach (var bullet in bullets.GetAll())
            buffer.Pixels[bullet.Position.ToPixelPosition().ToPixelIndex()] = true;
    }
}
