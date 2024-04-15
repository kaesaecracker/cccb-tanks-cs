using DisplayCommands;
using TanksServer.GameLogic;

namespace TanksServer.Graphics;

internal sealed class GeneratePixelsTickStep(
    IEnumerable<IDrawStep> drawSteps,
    IEnumerable<IFrameConsumer> consumers
) : ITickStep
{
    private readonly List<IDrawStep> _drawSteps = drawSteps.ToList();
    private readonly List<IFrameConsumer> _consumers = consumers.ToList();

    private readonly PixelGrid _observerPixelGrid = new(MapService.PixelsPerRow, MapService.PixelsPerColumn);
    private readonly GamePixelGrid _gamePixelGrid = new(MapService.PixelsPerRow, MapService.PixelsPerColumn);

    public async Task TickAsync()
    {
        _gamePixelGrid.Clear();
        foreach (var step in _drawSteps)
            step.Draw(_gamePixelGrid);

        _observerPixelGrid.Clear();
        for (var y = 0; y < MapService.PixelsPerColumn; y++)
        for (var x = 0; x < MapService.PixelsPerRow; x++)
        {
            if (_gamePixelGrid[x, y].EntityType.HasValue)
                _observerPixelGrid[(ushort)x, (ushort)y] = true;
        }

        foreach (var consumer in _consumers)
            await consumer.OnFrameDoneAsync(_gamePixelGrid, _observerPixelGrid);
    }
}
