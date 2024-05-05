using System.Diagnostics;

namespace TanksServer.GameLogic;

internal sealed class CollectPowerUp(
    MapEntityManager entityManager
) : ITickStep
{
    private readonly Predicate<PowerUp> _collectPredicate = b => TryCollect(b, entityManager.Tanks);

    public ValueTask TickAsync(TimeSpan delta)
    {
        entityManager.RemoveWhere(_collectPredicate);
        return ValueTask.CompletedTask;
    }

    private static bool TryCollect(PowerUp powerUp, IEnumerable<Tank> tanks)
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

    private static void ApplyPowerUpEffect(PowerUp powerUp, Tank tank)
    {
        switch (powerUp.Type)
        {
            case PowerUpType.MagazineType:
                if (powerUp.MagazineType == null)
                    throw new UnreachableException();

                tank.Magazine = tank.Magazine with
                {
                    Type = tank.Magazine.Type | powerUp.MagazineType.Value,
                    UsedBullets = 0
                };

                if (tank.ReloadingUntil >= DateTime.Now)
                    tank.ReloadingUntil = DateTime.Now;

                break;
            case PowerUpType.MagazineSize:
                tank.Magazine = tank.Magazine with
                {
                    MaxBullets = (byte)int.Clamp(tank.Magazine.MaxBullets + 1, 1, 32)
                };
                break;
            default:
                throw new UnreachableException();
        }
    }
}
