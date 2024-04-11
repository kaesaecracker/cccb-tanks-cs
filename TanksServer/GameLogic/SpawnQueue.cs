using System.Diagnostics.CodeAnalysis;

namespace TanksServer.GameLogic;

internal sealed class SpawnQueue(
    IOptions<PlayersConfiguration> options
)
{
    private ConcurrentQueue<Player> _queue = new();
    private ConcurrentDictionary<Player, DateTime> _spawnTimes = new();
    private readonly TimeSpan _spawnDelay = TimeSpan.FromMilliseconds(options.Value.SpawnDelayMs);

    public void EnqueueForImmediateSpawn(Player player)
    {
        _queue.Enqueue(player);
    }

    public void EnqueueForDelayedSpawn(Player player)
    {
        _queue.Enqueue(player);
        _spawnTimes.AddOrUpdate(player, DateTime.MinValue, (_, _) => DateTime.Now + _spawnDelay);
    }

    public bool TryDequeueNext([MaybeNullWhen(false)] out Player player)
    {
        if (!_queue.TryDequeue(out player))
            return false;

        var now = DateTime.Now;
        if (_spawnTimes.GetOrAdd(player, DateTime.MinValue) <= now)
            return true;

        _queue.Enqueue(player);
        return false;
    }
}