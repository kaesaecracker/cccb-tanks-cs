using DisplayCommands;
using TanksServer.GameLogic;

namespace TanksServer.Graphics;

internal sealed class BulletDrawer(BulletManager bullets) : IDrawStep
{
    public void Draw(PixelGrid buffer)
    {
        foreach (var bullet in bullets.GetAll())
        {
            var pos = bullet.Position.ToPixelPosition();
            buffer[pos.X, pos.Y] = true;
        }
    }
}