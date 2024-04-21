namespace TanksServer.Models;

internal sealed class PlayerControls
{
    public bool Forward { get; set; }
    public bool Backward { get; set; }
    public bool TurnLeft { get; set; }
    public bool TurnRight { get; set; }
    public bool Shoot { get; set; }
}