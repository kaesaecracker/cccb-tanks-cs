namespace TanksServer.GameLogic;

internal sealed class GameRules
{
    public bool DestructibleWalls { get; set; } = true;

    public double PowerUpSpawnChance { get; set; }

    public int MaxPowerUpCount { get; set; } = int.MaxValue;

    public int BulletTimeoutMs { get; set; } = int.MaxValue;

    public double MoveSpeed { get; set; }

    public double TurnSpeed { get; set; }

    public double ShootDelayMs { get; set; }

    public double BulletSpeed { get; set; } = 75;

    public int SpawnDelayMs { get; set; }

    public int IdleTimeoutMs { get; set; }

    public byte MagazineSize { get; set; } = 5;

    public int ReloadDelayMs { get; set; } = 3000;

    public double SmartBulletInertia { get; set; } = 1;

    public double BulletAccelerationUpgradeStrength { get; set; } = 15;

    public double BulletSpeedUpgradeStrength { get; set; } = 5;
}
