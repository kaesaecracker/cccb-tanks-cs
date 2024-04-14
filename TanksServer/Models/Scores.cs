namespace TanksServer.Models;

internal sealed record class Scores(int Kills = 0, int Deaths = 0)
{
    public int Kills { get; set; } = Kills;

    public int Deaths { get; set; } = Deaths;

    public double Ratio
    {
        get
        {
            if (Kills == 0)
                return 0;
            if (Deaths == 0)
                return Kills;
            return Kills / (double)Deaths;
        }
    }
}
