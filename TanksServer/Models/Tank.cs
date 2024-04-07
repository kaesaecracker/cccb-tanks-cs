namespace TanksServer.Models;

internal sealed class Tank(Player player, PixelPosition spawnPosition)
{
    public Player Owner { get; } = player;
    public int Rotation { get; set; }
    public PixelPosition Position { get; set; } = spawnPosition;
}
