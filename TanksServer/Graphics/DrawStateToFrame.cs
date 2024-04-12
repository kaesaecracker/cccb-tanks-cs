using DisplayCommands;
using TanksServer.GameLogic;

namespace TanksServer.Graphics;

internal sealed class DrawStateToFrame(
    IEnumerable<IDrawStep> drawSteps, LastFinishedFrameProvider lastFrameProvider
) : ITickStep
{
    private readonly List<IDrawStep> _drawSteps = drawSteps.ToList();
    private readonly PixelGrid _drawGrid = new(MapService.PixelsPerRow, MapService.PixelsPerColumn);

    public Task TickAsync()
    {
        // TODO: fix race condition with shared buffer access
        _drawGrid.Clear();
        foreach (var step in _drawSteps)
            step.Draw(_drawGrid);
        lastFrameProvider.LastFrame = _drawGrid;
        return Task.CompletedTask;
    }
}
