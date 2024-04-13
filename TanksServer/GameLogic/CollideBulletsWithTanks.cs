namespace TanksServer.GameLogic;

internal sealed class CollideBulletsWithTanks(
    BulletManager bullets,
    TankManager tanks,
    SpawnQueue spawnQueue
) : ITickStep
{
    public Task TickAsync()
    {
        bullets.RemoveWhere(BulletHitsTank);
        return Task.CompletedTask;
    }

    private bool BulletHitsTank(Bullet bullet)
    {
        foreach (var tank in tanks)
        {
            var (topLeft, bottomRight) = tank.Bounds;
            if (bullet.Position.X < topLeft.X || bullet.Position.X > bottomRight.X ||
                bullet.Position.Y < topLeft.Y || bullet.Position.Y > bottomRight.Y)
                continue;

            if (bullet.Owner != tank.Owner)
                bullet.Owner.Scores.Kills++;
            tank.Owner.Scores.Deaths++;

            tanks.Remove(tank);
            spawnQueue.EnqueueForDelayedSpawn(tank.Owner);

            return true;
        }

        return false;
    }
}
