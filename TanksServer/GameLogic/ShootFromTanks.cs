namespace TanksServer.GameLogic;

internal sealed class ShootFromTanks(
    TankManager tanks,
    IOptions<TanksConfiguration> options,
    BulletManager bulletManager
) : ITickStep
{
    private readonly TanksConfiguration _config = options.Value;

    public Task TickAsync()
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

        var angle = tank.Rotation * 2 * Math.PI;
        var position = new FloatPosition(
            tank.Position.X + Math.Sin(angle) * _config.BulletSpeed,
            tank.Position.Y - Math.Cos(angle) * _config.BulletSpeed
        );

        bulletManager.Spawn(new Bullet(tank.Owner, position, tank.Rotation));
    }
}
