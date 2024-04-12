using DisplayCommands;

namespace TanksServer.Graphics;

internal interface IDrawStep
{
    void Draw(PixelGrid buffer);
}
