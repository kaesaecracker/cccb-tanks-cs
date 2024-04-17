namespace TanksServer.GameLogic;

internal sealed class SpawnPowerUp(
    IOptions<GameRules> options,
    MapEntityManager entityManager
) : ITickStep
{
    private readonly double _spawnChance = options.Value.PowerUpSpawnChance;

    public Task TickAsync(TimeSpan delta)
    {
        if (Random.Shared.NextDouble() > _spawnChance * delta.TotalSeconds)
            return Task.CompletedTask;

        entityManager.SpawnPowerUp();
        return Task.CompletedTask;
    }
}
