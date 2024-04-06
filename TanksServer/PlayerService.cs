using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace TanksServer;

internal sealed class PlayerService(ILogger<PlayerService> logger)
{
    private readonly ConcurrentDictionary<string, Player> _players = new();

    public Player GetOrAdd(string name) => _players.GetOrAdd(name, _ => new Player(name));
}

internal class Player(string name)
{
    public string Name => name;

    public Guid Id => Guid.NewGuid();
}
