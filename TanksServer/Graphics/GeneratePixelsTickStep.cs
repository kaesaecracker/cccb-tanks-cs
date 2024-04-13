using DisplayCommands;
using TanksServer.GameLogic;

namespace TanksServer.Graphics;

internal sealed class GeneratePixelsTickStep(
    IEnumerable<IDrawStep> drawSteps,
    LastFinishedFrameProvider lastFrameProvider
) : ITickStep
{
    private readonly List<IDrawStep> _drawSteps = drawSteps.ToList();

    public Task TickAsync()
    {
        var drawGrid = new PixelGrid(MapService.PixelsPerRow, MapService.PixelsPerColumn);
        foreach (var step in _drawSteps)
            step.Draw(drawGrid);
        lastFrameProvider.LastFrame = drawGrid;
        return Task.CompletedTask;
    }
}
