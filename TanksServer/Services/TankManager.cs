using System.Collections;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace TanksServer.Services;

internal sealed class TankManager(ILogger<TankManager> logger) : IEnumerable<Tank>
{
    private readonly ConcurrentBag<Tank> _tanks = new();

    public void Add(Tank tank)
    {
        logger.LogInformation("Tank added for player {}", tank.Owner.Id);
        _tanks.Add(tank);
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public IEnumerator<Tank> GetEnumerator() => _tanks.GetEnumerator();
}
