using System.Collections;

namespace TanksServer.GameLogic;

internal sealed class TankManager(ILogger<TankManager> logger) : IEnumerable<Tank>
{
    private readonly ConcurrentDictionary<Tank, byte> _tanks = new();

    public void Add(Tank tank)
    {
        logger.LogInformation("Tank added for player {}", tank.Owner.Id);
        _tanks.TryAdd(tank, 0);
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public IEnumerator<Tank> GetEnumerator() => _tanks.Keys.GetEnumerator();

    public void Remove(Tank tank)
    {
        logger.LogInformation("Tank removed for player {}", tank.Owner.Id);
        _tanks.Remove(tank, out _);
    }

    public static (PixelPosition TopLeft, PixelPosition BottomRight) GetTankBounds(PixelPosition tankPosition)
    {
        return (tankPosition, new PixelPosition(
            (ushort)(tankPosition.X + MapService.TileSize - 1),
            (ushort)(tankPosition.Y + MapService.TileSize - 1)
        ));
    }
}