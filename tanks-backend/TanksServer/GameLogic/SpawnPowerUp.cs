namespace TanksServer.GameLogic;

internal sealed class SpawnPowerUp(
    IOptions<GameRules> options,
    MapEntityManager entityManager
) : ITickStep
{
    private readonly double _spawnChance = options.Value.PowerUpSpawnChance;
    private readonly int _maxCount = options.Value.MaxPowerUpCount;

    public ValueTask TickAsync(TimeSpan delta)
    {
        if (entityManager.PowerUps.Count() >= _maxCount)
            return ValueTask.CompletedTask;
        if (Random.Shared.NextDouble() > _spawnChance * delta.TotalSeconds)
            return ValueTask.CompletedTask;

        entityManager.SpawnPowerUp();
        return ValueTask.CompletedTask;
    }
}
