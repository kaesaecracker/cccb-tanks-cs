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
    private readonly Dictionary<string, Player> _players = [];
    private readonly SemaphoreSlim _mutex = new(1, 1);

    public Player GetOrAdd(string name)
    {
        _mutex.Wait();
        try
        {
            if (_players.TryGetValue(name, out var existingPlayer))
            {
                logger.LogInformation("player {} rejoined", existingPlayer.Name);
                return existingPlayer;
            }

            var newPlayer = new Player { Name = name };
            logger.LogInformation("player {} joined", newPlayer.Name);
            _players.Add(name, newPlayer);
            tankSpawnQueue.EnqueueForImmediateSpawn(newPlayer);
            return newPlayer;
        }
        finally
        {
            _mutex.Release();
        }
    }

    public bool TryGet(string name, [MaybeNullWhen(false)] out Player foundPlayer)
    {
        _mutex.Wait();
        try
        {
            foundPlayer = _players.Values.FirstOrDefault(player => player.Name == name);
            return foundPlayer != null;
        }
        finally
        {
            _mutex.Release();
        }
    }

    public List<Player> GetAll()
    {
        _mutex.Wait();
        try
        {
            return _players.Values.ToList();
        }
        finally
        {
            _mutex.Release();
        }
    }

    public Task HandleClientAsync(WebSocket webSocket, Player player)
        => HandleClientAsync(new PlayerInfoConnection(player, connectionLogger, webSocket, entityManager));

    public ValueTask TickAsync(TimeSpan delta)
        => ParallelForEachConnectionAsync(connection => connection.OnGameTickAsync().AsTask());
}
