namespace TanksServer.Models;

internal sealed record class Scores(int Kills = 0, int Deaths = 0)
{
    public int Kills { get; set; } = Kills;

    public int Deaths { get; set; } = Deaths;
}
