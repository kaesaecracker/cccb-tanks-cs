using TanksServer.GameLogic;

namespace TanksServer.Models;

internal readonly struct TilePosition(ushort x, ushort y)
{
    public ushort X { get; } = (ushort)((x + MapService.TilesPerRow) % MapService.TilesPerRow);
    public ushort Y { get; } = (ushort)((y + MapService.TilesPerColumn) % MapService.TilesPerColumn);
}