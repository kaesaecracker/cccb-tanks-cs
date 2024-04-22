namespace TanksServer.Models;

internal sealed record class PlayerInfo(string Name, Scores Scores, PlayerControls Controls);
