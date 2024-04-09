using TanksServer.ServicePointDisplay;

namespace TanksServer.DrawSteps;

internal interface IDrawStep
{
    void Draw(PixelDisplayBufferView buffer);
}
