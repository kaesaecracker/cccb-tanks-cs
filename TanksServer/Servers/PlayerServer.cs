using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using TanksServer.TickSteps;

namespace TanksServer.Servers;

internal sealed class PlayerServer(ILogger<PlayerServer> logger, SpawnNewTanks spawnNewTanks)
{
    private readonly ConcurrentDictionary<string, Player> _players = new();

    public Player GetOrAdd(string name)
    {
        var player = _players.GetOrAdd(name, AddAndSpawn);
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
    
    private Player AddAndSpawn(string name)
    {
        var player = new Player(name);
        spawnNewTanks.SpawnTankForPlayer(player);
        return player;
    }
}
