using TanksServer.ServicePointDisplay;

namespace TanksServer.Graphics;

internal interface IDrawStep
{
    void Draw(PixelDisplayBufferView buffer);
}
