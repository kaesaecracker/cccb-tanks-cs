namespace TanksServer.GameLogic;

internal sealed class SpawnQueueProvider
{
    public ConcurrentQueue<Player> Queue { get; } = new();
}
