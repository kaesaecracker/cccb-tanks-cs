using DisplayCommands;
using TanksServer.GameLogic;

namespace TanksServer.Graphics;

internal sealed class DrawBulletsStep(BulletManager bullets) : IDrawStep
{
    public void Draw(PixelGrid buffer)
    {
        foreach (var position in bullets.GetAll().Select(b => b.Position.ToPixelPosition()))
            buffer[position.X, position.Y] = true;
    }
}
