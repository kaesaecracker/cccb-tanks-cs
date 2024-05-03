using System.Diagnostics;

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


        var type = Random.Shared.Next(4) == 0
            ? PowerUpType.MagazineSize
            : PowerUpType.MagazineType;

        MagazineType? magazineType = type switch
        {
            PowerUpType.MagazineType => Random.Shared.Next(0, 3) switch
            {
                0 => MagazineType.Fast,
                1 => MagazineType.Explosive,
                2 => MagazineType.Smart,
                _ => throw new UnreachableException()
            },
            _ => null
        };

        var position = emptyTileFinder.ChooseEmptyTile().GetCenter().ToFloatPosition();
        entityManager.SpawnPowerUp(position, type, magazineType);
        return ValueTask.CompletedTask;
    }
}
