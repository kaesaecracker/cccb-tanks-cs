using System.Diagnostics.CodeAnalysis;
using System.Net.WebSockets;
using TanksServer.GameLogic;

namespace TanksServer.Interactivity;

internal sealed class PlayerServer(
    ILogger<PlayerServer> logger,
    ILogger<PlayerInfoConnection> connectionLogger,
    TankSpawnQueue tankSpawnQueue,
    MapEntityManager entityManager
) : WebsocketServer<PlayerInfoConnection>(logger), ITickStep
{
    private readonly ConcurrentDictionary<string, Player> _players = new();

    public Player? GetOrAdd(string name, Guid id)
    {
        var existingOrAddedPlayer = _players.GetOrAdd(name, _ => AddAndSpawn());
        if (existingOrAddedPlayer.Id != id)
            return null;

        logger.LogInformation("player {} (re)joined", existingOrAddedPlayer.Id);
        return existingOrAddedPlayer;

        Player AddAndSpawn()
        {
            var newPlayer = new Player(name, id);
            tankSpawnQueue.EnqueueForImmediateSpawn(newPlayer);
            return newPlayer;
        }
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

    public Task HandleClientAsync(WebSocket webSocket, Player player)
        => HandleClientAsync(new PlayerInfoConnection(player, connectionLogger, webSocket, entityManager));

    public Task TickAsync(TimeSpan delta)
        => ParallelForEachConnectionAsync(connection => connection.OnGameTickAsync());
}
