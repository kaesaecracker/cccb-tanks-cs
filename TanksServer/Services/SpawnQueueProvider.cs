using TanksServer.Models;

namespace TanksServer.Services;

internal sealed class SpawnQueueProvider
{
    public ConcurrentQueue<Player> Queue { get; } = new();
}
