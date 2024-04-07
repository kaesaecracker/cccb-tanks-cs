using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using TanksServer.Models;
using TanksServer.Services;

namespace TanksServer;

internal sealed class PlayerServer(ILogger<PlayerServer> logger, SpawnQueue spawnQueue)
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
        spawnQueue.SpawnTankForPlayer(player);
        return player;
    }
}
