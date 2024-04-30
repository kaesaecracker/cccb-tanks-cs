using System.Diagnostics;

namespace TanksServer.GameLogic;

internal sealed class CollectPowerUp(
    MapEntityManager entityManager
) : ITickStep
{
    public ValueTask TickAsync(TimeSpan delta)
    {
        entityManager.RemoveWhere(TryCollect);
        return ValueTask.CompletedTask;
    }

    private bool TryCollect(PowerUp obj)
    {
        var position = obj.Position;
        foreach (var tank in entityManager.Tanks)
        {
            var (topLeft, bottomRight) = tank.Bounds;
            if (position.X < topLeft.X || position.X > bottomRight.X ||
                position.Y < topLeft.Y || position.Y > bottomRight.Y)
                continue;

            // now the tank overlaps the power up by at least 0.5 tiles

            switch (obj.Type)
            {
                case PowerUpType.MagazineType:
                    if (obj.MagazineType == null)
                        throw new UnreachableException();

                    tank.Magazine = tank.Magazine with
                    {
                        Type = tank.Magazine.Type | obj.MagazineType.Value,
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

            tank.Owner.Scores.PowerUpsCollected++;
            return true;
        }

        return false;
    }
}
