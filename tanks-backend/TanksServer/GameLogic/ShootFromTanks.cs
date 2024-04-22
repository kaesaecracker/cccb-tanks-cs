using System.Diagnostics;

namespace TanksServer.GameLogic;

internal sealed class ShootFromTanks(
    IOptions<GameRules> options,
    MapEntityManager entityManager
) : ITickStep
{
    private readonly GameRules _config = options.Value;

    public Task TickAsync(TimeSpan _)
    {
        foreach (var tank in entityManager.Tanks.Where(t => !t.Moving))
            Shoot(tank);

        return Task.CompletedTask;
    }

    private void Shoot(Tank tank)
    {
        if (!tank.Owner.Controls.Shoot)
            return;
        if (tank.NextShotAfter >= DateTime.Now)
            return;

        tank.NextShotAfter = DateTime.Now.AddMilliseconds(_config.ShootDelayMs);

        var explosive = tank.ExplosiveBullets > 0;
        if (explosive)
            tank.ExplosiveBullets--;

        entityManager.SpawnBullet(tank.Owner, tank.Position, tank.Orientation / 16d, explosive);
    }
}
