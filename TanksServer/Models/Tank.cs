namespace TanksServer.Models;

internal sealed class Tank(Player player, FloatPosition spawnPosition)
{
    private double _rotation;

    public Player Owner { get; } = player;

    /// <summary>
    /// Bounds: 0 (inclusive) .. 16 (exclusive)
    /// </summary>
    public double Rotation
    {
        get => _rotation;
        set => _rotation = (value + 16d) % 16d;
    }

    public FloatPosition Position { get; set; } = spawnPosition;
    
    public DateTime NextShotAfter { get; set; }
}
