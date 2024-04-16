namespace TanksServer.GameLogic;

internal sealed class ShootFromTanks(
    TankManager tanks,
    IOptions<TanksConfiguration> options,
    BulletManager bulletManager
) : ITickStep
{
    private readonly TanksConfiguration _config = options.Value;

    public Task TickAsync(TimeSpan _)
    {
        foreach (var tank in tanks.Where(t => !t.Moved))
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

        var rotation = tank.Orientation / 16d;
        var angle = rotation * 2d * Math.PI;
        var position = new FloatPosition(
            tank.Position.X + Math.Sin(angle) * 6,
            tank.Position.Y - Math.Cos(angle) * 6
        );

        bulletManager.Spawn(new Bullet(tank.Owner, position, rotation));
    }
}
