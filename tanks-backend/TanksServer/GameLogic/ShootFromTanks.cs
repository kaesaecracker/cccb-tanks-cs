namespace TanksServer.GameLogic;

internal sealed class ShootFromTanks(
    IOptions<GameRules> options,
    MapEntityManager entityManager
) : ITickStep
{
    private readonly GameRules _config = options.Value;

    public ValueTask TickAsync(TimeSpan _)
    {
        foreach (var tank in entityManager.Tanks.Where(t => !t.Moving))
            Shoot(tank);

        return ValueTask.CompletedTask;
    }

    private void Shoot(Tank tank)
    {
        if (!tank.Owner.Controls.Shoot)
            return;

        var now = DateTime.Now;
        if (tank.NextShotAfter >= now)
            return;
        if (tank.ReloadingUntil >= now)
            return;

        if (tank.Magazine.Empty)
        {
            tank.ReloadingUntil = now.AddMilliseconds(_config.ReloadDelayMs);
            tank.Magazine = tank.Magazine with
            {
                UsedBullets = 0,
                Type = MagazineType.Basic
            };
            return;
        }

        tank.NextShotAfter = now.AddMilliseconds(_config.ShootDelayMs);
        tank.Magazine = tank.Magazine with
        {
            UsedBullets = (byte)(tank.Magazine.UsedBullets + 1)
        };

        tank.Owner.Scores.ShotsFired++;
        entityManager.SpawnBullet(tank.Owner, tank.Position, tank.Orientation / 16d, tank.Magazine.Type);
    }
}
