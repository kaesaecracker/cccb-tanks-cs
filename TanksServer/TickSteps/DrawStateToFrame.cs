using TanksServer.DrawSteps;
using TanksServer.ServicePointDisplay;
using TanksServer.Services;

namespace TanksServer.TickSteps;

internal sealed class DrawStateToFrame(
    IEnumerable<IDrawStep> drawSteps, LastFinishedFrameProvider lastFrameProvider
) : ITickStep
{
    private readonly List<IDrawStep> _drawSteps = drawSteps.ToList();

    public Task TickAsync()
    {
        var buffer = PixelDisplayBufferView.New(0, 0, MapService.TilesPerRow, MapService.PixelsPerColumn);
        foreach (var step in _drawSteps)
            step.Draw(buffer);
        lastFrameProvider.LastFrame = buffer;
        return Task.CompletedTask;
    }
}
