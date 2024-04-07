using TanksServer.Models;
using TanksServer.Services;

namespace TanksServer.TickSteps;

internal sealed class CollideBulletsWithTanks(
    BulletManager bullets, TankManager tanks, SpawnQueueProvider spawnQueueProvider
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
            var (topLeft, bottomRight) = tank.GetBounds();
            if (bullet.Position.X < topLeft.X || bullet.Position.X > bottomRight.X ||
                bullet.Position.Y < topLeft.Y || bullet.Position.Y > bottomRight.Y)
                continue;

            if (bullet.Owner != tank.Owner)
                bullet.Owner.Kills++;
            tank.Owner.Deaths++;

            tanks.Remove(tank);
            spawnQueueProvider.Queue.Enqueue(tank.Owner);

            return true;
        }

        return false;
    }
}
