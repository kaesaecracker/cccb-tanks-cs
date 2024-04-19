namespace TanksServer.GameLogic;

internal sealed class GameRules
{
    public bool DestructibleWalls { get; set; } = true;

    public double PowerUpSpawnChance { get; set; }

    public int MaxPowerUpCount { get; set; } = int.MaxValue;

    public int BulletTimeoutMs { get; set; } = int.MaxValue;
}
