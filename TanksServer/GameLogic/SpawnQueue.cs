using System.Diagnostics.CodeAnalysis;

namespace TanksServer.GameLogic;

internal sealed class SpawnQueue(
    IOptions<PlayersConfiguration> options
)
{
    private readonly ConcurrentQueue<Player> _queue = new();
    private readonly ConcurrentDictionary<Player, DateTime> _spawnTimes = new();
    private readonly TimeSpan _spawnDelay = TimeSpan.FromMilliseconds(options.Value.SpawnDelayMs);
    private readonly TimeSpan _idleTimeout = TimeSpan.FromMilliseconds(options.Value.IdleTimeoutMs);

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
            return false; // no one on queue

        if (player.LastInput + _idleTimeout < DateTime.Now)
        {
            // player idle
            _queue.Enqueue(player);
            return false;
        }
        
        var now = DateTime.Now;
        if (_spawnTimes.GetOrAdd(player, DateTime.MinValue) > now)
        {
            // spawn delay
            _queue.Enqueue(player);
            return false;
        }

        return true;
    }
}