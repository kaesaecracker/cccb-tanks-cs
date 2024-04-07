using TanksServer.DrawSteps;
using TanksServer.Helpers;
using TanksServer.TickSteps;

namespace TanksServer.Services;

internal sealed class PixelDrawer(IEnumerable<IDrawStep> drawSteps) : ITickStep
{
    private const uint GameFieldPixelCount = MapService.PixelsPerRow * MapService.PixelsPerColumn;
    private DisplayPixelBuffer? _lastFrame;
    private readonly List<IDrawStep> _drawSteps = drawSteps.ToList();

    public DisplayPixelBuffer LastFrame
    {
        get => _lastFrame ?? throw new InvalidOperationException("first frame not yet drawn");
        private set => _lastFrame = value;
    }

    public Task TickAsync()
    {
        var buffer = CreateGameFieldPixelBuffer();
        foreach (var step in _drawSteps) 
            step.Draw(buffer);
        LastFrame = buffer;
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
