using System.Text.Json.Serialization;

namespace TanksServer.Models;

internal sealed record class Scores
{
    public int Kills { get; set; }

    public int Deaths { get; set; }

    public double Ratio
    {
        get
        {
            if (Kills == 0)
                return 0;
            if (Deaths == 0)
                return Kills;
            return Math.Round(Kills / (double)Deaths, 3);
        }
    }

    public int WallsDestroyed { get; set; }

    public int ShotsFired { get; set; }

    public int PowerUpsCollected { get; set; }

    [JsonIgnore] public double DistanceMoved { get; set; }

    public int PixelsMoved => (int)DistanceMoved;

    public int OverallScore => Math.Max(0,
        10000 * Kills
        - 1000 * Deaths
        + 100 * PowerUpsCollected
        + 10 * (ShotsFired + WallsDestroyed)
        + PixelsMoved
    );
}
