using System.Diagnostics;

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

        /* TODO: when standing next to a wall, the bullet sometimes misses the first pixel.
         Spawning the bullet to close to the tank instead means the tank instantly hits itself.
         Because the tank has a float position, but hit boxes are based on pixels, this problem has been deemed complex
         enough to do later. These values mostly work. */
        var distance = (tank.Orientation % 4) switch
        {
            0 => 4.4d,
            1 or 3 => 5.4d,
            2 => 6d,
            _ => throw new UnreachableException("this should not be possible")
        };

        var position = new FloatPosition(
            tank.Position.X + Math.Sin(angle) * distance,
            tank.Position.Y - Math.Cos(angle) * distance
        );

        bulletManager.Spawn(tank.Owner, position, rotation);
    }
}
