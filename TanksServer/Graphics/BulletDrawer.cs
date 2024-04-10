using TanksServer.GameLogic;
using TanksServer.ServicePointDisplay;

namespace TanksServer.Graphics;

internal sealed class BulletDrawer(BulletManager bullets): IDrawStep
{
    public void Draw(PixelDisplayBufferView buffer)
    {
        foreach (var bullet in bullets.GetAll())
            buffer.Pixels[bullet.Position.ToPixelPosition().ToPixelIndex()] = true;
    }
}
