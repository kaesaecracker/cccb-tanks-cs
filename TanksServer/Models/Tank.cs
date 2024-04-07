namespace TanksServer.Models;

internal sealed class Tank(Player player, FloatPosition spawnPosition)
{
    private double _rotation;
    
    public Player Owner { get; } = player;

    public double Rotation
    {
        get => _rotation;
        set => _rotation = value % 16d;
    }

    public FloatPosition Position { get; set; } = spawnPosition;
}
