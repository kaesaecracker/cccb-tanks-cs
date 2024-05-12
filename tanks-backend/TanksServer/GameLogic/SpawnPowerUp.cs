namespace TanksServer.GameLogic;

internal sealed class SpawnPowerUp(
    IOptions<GameRules> options,
    MapEntityManager entityManager,
    EmptyTileFinder emptyTileFinder
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

        var type = (PowerUpType)Random.Shared.Next((int)Enum.GetValues<PowerUpType>().Max());
        var position = emptyTileFinder.ChooseEmptyTile().GetCenter().ToFloatPosition();
        entityManager.SpawnPowerUp(position, type);
        return ValueTask.CompletedTask;
    }
}
