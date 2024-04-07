namespace TanksServer.TickSteps;

internal sealed class ShootFromTanks(
    TankManager tanks, IOptions<TanksConfiguration> options, BulletManager bulletManager
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

        var angle = tank.Rotation / 16 * 2 * Math.PI;
        var position = new FloatPosition(
            X: tank.Position.X + MapService.TileSize / 2d + Math.Sin(angle) * _config.BulletSpeed,
            Y: tank.Position.Y + MapService.TileSize / 2d - Math.Cos(angle) * _config.BulletSpeed
        );

        bulletManager.Spawn(new Bullet(tank.Owner, position, tank.Rotation));
    }
}
