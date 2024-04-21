using TanksServer.GameLogic;

namespace TanksServer.Models;

internal sealed class PowerUp(FloatPosition position): IMapEntity
{
    public FloatPosition Position { get; set; } = position;

    public PixelBounds Bounds => Position.GetBoundsForCenter(MapService.TileSize);
}
