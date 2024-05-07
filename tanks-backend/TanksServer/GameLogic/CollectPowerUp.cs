namespace TanksServer.GameLogic;

internal sealed class CollectPowerUp : ITickStep
{
    private readonly Predicate<PowerUp> _collectPredicate;
    private readonly GameRules _rules;
    private readonly MapEntityManager _entityManager;

    public CollectPowerUp(MapEntityManager entityManager,
        IOptions<GameRules> options)
    {
        _entityManager = entityManager;
        _rules = options.Value;
        _collectPredicate = b => TryCollect(b, entityManager.Tanks);
    }

    public ValueTask TickAsync(TimeSpan delta)
    {
        _entityManager.RemoveWhere(_collectPredicate);
        return ValueTask.CompletedTask;
    }

    private bool TryCollect(PowerUp powerUp, IEnumerable<Tank> tanks)
    {
        var position = powerUp.Position;
        foreach (var tank in tanks)
        {
            var (topLeft, bottomRight) = tank.Bounds;
            if (position.X < topLeft.X || position.X > bottomRight.X ||
                position.Y < topLeft.Y || position.Y > bottomRight.Y)
                continue;

            // now the tank overlaps the power up by at least 0.5 tiles

            ApplyPowerUpEffect(powerUp, tank);
            tank.Owner.Scores.PowerUpsCollected++;
            return true;
        }

        return false;
    }

    private void ApplyPowerUpEffect(PowerUp powerUp, Tank tank)
    {
        switch (powerUp.Type)
        {
            case PowerUpType.MagazineSize:
                tank.MaxBullets = int.Clamp(tank.MaxBullets + 1, 1, 32);
                break;

            case PowerUpType.BulletAcceleration:
                tank.BulletStats = tank.BulletStats with
                {
                    Acceleration = tank.BulletStats.Acceleration + _rules.BulletAccelerationUpgradeStrength
                };
                break;

            case PowerUpType.ExplosiveBullets:
                tank.BulletStats = tank.BulletStats with { Explosive = true };
                break;

            case PowerUpType.SmartBullets:
                tank.BulletStats = tank.BulletStats with { Smart = true };
                break;

            case PowerUpType.BulletSpeed:
                tank.BulletStats = tank.BulletStats with
                {
                    Speed = tank.BulletStats.Speed + _rules.BulletSpeedUpgradeStrength
                };
                break;

            default:
                throw new NotImplementedException($"unknown type {powerUp.Type}");
        }
    }
}
