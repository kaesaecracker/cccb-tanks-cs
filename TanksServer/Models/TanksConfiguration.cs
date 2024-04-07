namespace TanksServer.Models;

public class TanksConfiguration
{
    public double MoveSpeed { get; set; } = 1.4;
    public double TurnSpeed { get; set; } = 0.4;
    public double ShootDelayMs { get; set; } = 0.4 * 1000;
    public double BulletSpeed { get; set; } = 8;
}
