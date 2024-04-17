using System.Diagnostics.CodeAnalysis;
using TanksServer.GameLogic;

namespace TanksServer.Interactivity;

internal sealed class PlayerServer(ILogger<PlayerServer> logger, TankSpawnQueue tankSpawnQueue)
{
    private readonly ConcurrentDictionary<string, Player> _players = new();

    public Player? GetOrAdd(string name, Guid id)
    {
        Player AddAndSpawn()
        {
            var player = new Player(name, id);
            tankSpawnQueue.EnqueueForImmediateSpawn(player);
            return player;
        }

        var player = _players.GetOrAdd(name, _ => AddAndSpawn());
        if (player.Id != id)
            return null;

        logger.LogInformation("player {} (re)joined", player.Id);
        return player;
    }

    public bool TryGet(Guid? playerId, [MaybeNullWhen(false)] out Player foundPlayer)
    {
        foreach (var player in _players.Values)
        {
            if (player.Id != playerId)
                continue;
            foundPlayer = player;
            return true;
        }

        foundPlayer = null;
        return false;
    }

    public IEnumerable<Player> GetAll() => _players.Values;
}
