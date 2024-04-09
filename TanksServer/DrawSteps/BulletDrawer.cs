using TanksServer.Helpers;
using TanksServer.ServicePointDisplay;
using TanksServer.Services;

namespace TanksServer.DrawSteps;

internal sealed class BulletDrawer(BulletManager bullets): IDrawStep
{
    public void Draw(PixelDisplayBufferView buffer)
    {
        foreach (var bullet in bullets.GetAll())
            buffer.Pixels[bullet.Position.ToPixelPosition().ToPixelIndex()] = true;
    }
}
