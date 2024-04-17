namespace TanksServer.GameLogic;

internal sealed class SpawnPowerUp(
    IOptions<GameRules> options,
    MapEntityManager entityManager
) : ITickStep
{
    private readonly double _spawnChance = options.Value.PowerUpSpawnChance;
    private readonly int _maxCount = options.Value.MaxPowerUpCount;

    public Task TickAsync(TimeSpan delta)
    {
        if (entityManager.PowerUps.Count() >= _maxCount)
            return Task.CompletedTask;
        if (Random.Shared.NextDouble() > _spawnChance * delta.TotalSeconds)
            return Task.CompletedTask;

        entityManager.SpawnPowerUp();
        return Task.CompletedTask;
    }
}
