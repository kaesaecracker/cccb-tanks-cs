using TanksServer.DrawSteps;
using TanksServer.Helpers;
using TanksServer.Services;

namespace TanksServer.TickSteps;

internal sealed class DrawStateToFrame(
    IEnumerable<IDrawStep> drawSteps, LastFinishedFrameProvider lastFrameProvider
) : ITickStep
{
    private const uint GameFieldPixelCount = MapService.PixelsPerRow * MapService.PixelsPerColumn;
    private readonly List<IDrawStep> _drawSteps = drawSteps.ToList();

    public Task TickAsync()
    {
        var buffer = CreateGameFieldPixelBuffer();
        foreach (var step in _drawSteps)
            step.Draw(buffer);
        lastFrameProvider.LastFrame = buffer;
        return Task.CompletedTask;
    }

    private static DisplayPixelBuffer CreateGameFieldPixelBuffer()
    {
        var data = new byte[10 + GameFieldPixelCount / 8];
        var result = new DisplayPixelBuffer(data)
        {
            Magic1 = 0,
            Magic2 = 19,
            X = 0,
            Y = 0,
            WidthInTiles = MapService.TilesPerRow,
            HeightInPixels = MapService.PixelsPerColumn
        };
        return result;
    }
}
