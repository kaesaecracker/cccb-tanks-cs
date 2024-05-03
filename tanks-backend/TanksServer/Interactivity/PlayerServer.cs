using System.Diagnostics.CodeAnalysis;
using System.Net.WebSockets;
using TanksServer.GameLogic;

namespace TanksServer.Interactivity;

internal sealed class PlayerServer(
    ILogger<PlayerServer> logger,
    ILogger<PlayerInfoConnection> connectionLogger,
    TankSpawnQueue tankSpawnQueue,
    MapEntityManager entityManager,
    BufferPool bufferPool
) : WebsocketServer<PlayerInfoConnection>(logger), ITickStep
{
    private readonly ConcurrentDictionary<string, Player> _players = [];

    public Player GetOrAdd(string name) => _players.GetOrAdd(name, Add);

    public bool TryGet(string name, [MaybeNullWhen(false)] out Player foundPlayer)
        => _players.TryGetValue(name, out foundPlayer);

    public IEnumerable<Player> Players => _players.Values;

    private Player Add(string name)
    {
        var newPlayer = new Player { Name = name };
        logger.LogInformation("player {} joined", newPlayer.Name);
        tankSpawnQueue.EnqueueForImmediateSpawn(newPlayer);
        return newPlayer;
    }

    public Task HandleClientAsync(WebSocket webSocket, Player player)
    {
        var connection = new PlayerInfoConnection(player, connectionLogger, webSocket, entityManager, bufferPool);
        return HandleClientAsync(connection);
    }

    public async ValueTask TickAsync(TimeSpan delta)
        => await Connections.Select(connection => connection.OnGameTickAsync())
            .WhenAll();
}
