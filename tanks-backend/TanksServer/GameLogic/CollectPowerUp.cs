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

            tank.Magazine = tank.Magazine with
            {
                UsedBullets = 0,
                Type = tank.Magazine.Type | MagazineType.Explosive
            };

            if (tank.ReloadingUntil >= DateTime.Now)
                tank.ReloadingUntil = DateTime.Now;

            tank.Owner.Scores.PowerUpsCollected++;
            return true;
        }

        return false;
    }
}
