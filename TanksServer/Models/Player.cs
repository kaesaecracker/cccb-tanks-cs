namespace TanksServer.Models;

internal sealed class Player(string name)
{
    public string Name => name;

    public Guid Id { get; } = Guid.NewGuid();

    public PlayerControls Controls { get; } = new();

    public int Kills { get; set; }
    
    public int Deaths { get; set; }
}

internal sealed class PlayerControls
{
    public bool Forward { get; set; }
    public bool Backward { get; set; }
    public bool TurnLeft { get; set; }
    public bool TurnRight { get; set; }
    public bool Shoot { get; set; }
}
