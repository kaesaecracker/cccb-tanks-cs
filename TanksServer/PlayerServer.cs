using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace TanksServer;

internal sealed class PlayerServer(ILogger<PlayerServer> logger)
{
    private readonly ConcurrentDictionary<string, Player> _players = new();

    public Player GetOrAdd(string name)
    {
        var player = _players.GetOrAdd(name, _ => new Player(name));
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
}

internal sealed class Player(string name)
{
    public string Name => name;

    public Guid Id { get; } = Guid.NewGuid();
    
    public PlayerControls Controls { get; } = new();
}

internal sealed class PlayerControls
{
    public bool Forward { get; set; }
    public bool Backward { get; set; }
    public bool TurnLeft { get; set; }
    public bool TurnRight { get; set; }
    public bool Shoot { get; set; }
}
