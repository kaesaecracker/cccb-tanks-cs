namespace TanksServer.GameLogic;

internal sealed class GameRules
{
    public bool DestructibleWalls { get; set; } = true;

    public double PowerUpSpawnChance { get; set; }
}
