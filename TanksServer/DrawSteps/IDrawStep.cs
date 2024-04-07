using TanksServer.Helpers;

namespace TanksServer.DrawSteps;

internal interface IDrawStep
{
    void Draw(DisplayPixelBuffer buffer);
}
