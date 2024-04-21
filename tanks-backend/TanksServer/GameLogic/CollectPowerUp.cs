namespace TanksServer.GameLogic;

internal sealed class CollectPowerUp(
    MapEntityManager entityManager
) : ITickStep
{
    public Task TickAsync(TimeSpan delta)
    {
        entityManager.RemoveWhere(TryCollect);
        return Task.CompletedTask;
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

            // this works because now the tank overlaps the power up
            tank.ExplosiveBullets += 10;
            return true;
        }

        return false;
    }
}
