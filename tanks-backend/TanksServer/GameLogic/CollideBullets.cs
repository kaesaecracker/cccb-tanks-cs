namespace TanksServer.GameLogic;

internal sealed class CollideBullets(
    MapEntityManager entityManager,
    MapService map,
    IOptions<GameRules> options,
    TankSpawnQueue tankSpawnQueue
) : ITickStep
{
    private const int ExplosionRadius = 3;

    public Task TickAsync(TimeSpan _)
    {
        entityManager.RemoveBulletsWhere(BulletHitsTank);
        entityManager.RemoveBulletsWhere(TryHitAndDestroyWall);
        entityManager.RemoveBulletsWhere(TimeoutBullet);
        return Task.CompletedTask;
    }

    private bool TimeoutBullet(Bullet bullet)
    {
        if (bullet.Timeout > DateTime.Now)
            return false;

        var radius = bullet.IsExplosive ? ExplosionRadius : 0;
        ExplodeAt(bullet.Position.ToPixelPosition(), radius, bullet.Owner);
        return true;
    }

    private bool TryHitAndDestroyWall(Bullet bullet)
    {
        var pixel = bullet.Position.ToPixelPosition();
        if (!map.Current.IsWall(pixel))
            return false;

        var radius = bullet.IsExplosive ? ExplosionRadius : 0;
        ExplodeAt(pixel, radius, bullet.Owner);

        return true;
    }

    private bool BulletHitsTank(Bullet bullet)
    {
        if (!TryHitTankAt(bullet.Position, bullet.Owner, DateTime.Now > bullet.OwnerCollisionAfter))
            return false;

        if (bullet.IsExplosive)
            ExplodeAt(bullet.Position.ToPixelPosition(), ExplosionRadius, bullet.Owner);
        return true;
    }

    private bool TryHitTankAt(FloatPosition position, Player owner, bool canHitOwnTank)
    {
        foreach (var tank in entityManager.Tanks)
        {
            var hitsOwnTank = owner == tank.Owner;
            if (hitsOwnTank && !canHitOwnTank)
                continue;

            var (topLeft, bottomRight) = tank.Bounds;
            if (position.X < topLeft.X || position.X > bottomRight.X ||
                position.Y < topLeft.Y || position.Y > bottomRight.Y)
                continue;

            if (!hitsOwnTank)
                owner.Scores.Kills++;
            tank.Owner.Scores.Deaths++;

            entityManager.Remove(tank);
            tankSpawnQueue.EnqueueForDelayedSpawn(tank.Owner);
            return true;
        }

        return false;
    }

    private void ExplodeAt(PixelPosition pixel, int i, Player owner)
    {
        for (var x = pixel.X - i; x <= pixel.X + i; x++)
        for (var y = pixel.Y - i; y <= pixel.Y + i; y++)
        {
            var offsetPixel = new PixelPosition(x, y);
            if (options.Value.DestructibleWalls && map.Current.TryDestroyWallAt(offsetPixel))
                owner.Scores.WallsDestroyed++;

            TryHitTankAt(offsetPixel.ToFloatPosition(), owner, true);
        }
    }
}
