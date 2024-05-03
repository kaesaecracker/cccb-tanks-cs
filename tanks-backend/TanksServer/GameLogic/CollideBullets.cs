using TanksServer.Graphics;

namespace TanksServer.GameLogic;

internal sealed class CollideBullets(
    MapEntityManager entityManager,
    MapService map,
    IOptions<GameRules> options,
    TankSpawnQueue tankSpawnQueue
) : ITickStep
{
    private readonly Sprite _explosiveSprite = Sprite.FromImageFile("assets/explosion.png");

    public ValueTask TickAsync(TimeSpan _)
    {
        entityManager.RemoveWhere(BulletHitsTank);
        entityManager.RemoveWhere(BulletHitsWall);
        entityManager.RemoveWhere(BulletTimesOut);
        return ValueTask.CompletedTask;
    }

    private bool BulletTimesOut(Bullet bullet)
    {
        if (bullet.Timeout > DateTime.Now)
            return false;

        ExplodeAt(bullet.Position.ToPixelPosition(), bullet.IsExplosive, bullet.Owner);
        return true;
    }

    private bool BulletHitsWall(Bullet bullet)
    {
        var pixel = bullet.Position.ToPixelPosition();
        if (!map.Current.IsWall(pixel))
            return false;

        ExplodeAt(pixel, bullet.IsExplosive, bullet.Owner);
        return true;
    }

    private bool BulletHitsTank(Bullet bullet)
    {
        var hitTank = GetTankAt(bullet.Position, bullet.Owner, DateTime.Now > bullet.OwnerCollisionAfter);
        if (hitTank == null)
            return false;

        ExplodeAt(bullet.Position.ToPixelPosition(), bullet.IsExplosive, bullet.Owner);
        return true;
    }

    private Tank? GetTankAt(FloatPosition position, Player owner, bool canHitOwnTank)
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

            return tank;
        }

        return null;
    }

    private void ExplodeAt(PixelPosition pixel, bool isExplosive, Player owner)
    {
        if (!isExplosive)
        {
            Core(pixel);
            return;
        }

        pixel = pixel.GetPixelRelative(-4, -4);
        for (short dx = 0; dx < _explosiveSprite.Width; dx++)
        for (short dy = 0; dy < _explosiveSprite.Height; dy++)
        {
            if (!_explosiveSprite[dx, dy].HasValue)
                continue;

            Core(pixel.GetPixelRelative(dx, dy));
        }

        return;

        void Core(PixelPosition position)
        {
            if (options.Value.DestructibleWalls && map.Current.TryDestroyWallAt(position))
                owner.Scores.WallsDestroyed++;

            var tank = GetTankAt(position.ToFloatPosition(), owner, true);
            if (tank == null)
                return;

            if (tank.Owner != owner)
                owner.Scores.Kills++;
            tank.Owner.Scores.Deaths++;

            entityManager.Remove(tank);
            tankSpawnQueue.EnqueueForDelayedSpawn(tank.Owner);
        }
    }
}
