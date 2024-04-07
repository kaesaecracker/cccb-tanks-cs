using System.Collections;
using TanksServer.Models;

namespace TanksServer.Services;

internal sealed class BulletManager : ITickStep
{
    private readonly HashSet<Bullet> _bullets = new();

    public void Spawn(Bullet bullet) => _bullets.Add(bullet);

    public Task TickAsync()
    {
        foreach (var bullet in _bullets)
        {
            MoveBullet(bullet);
        }

        return Task.CompletedTask;
    }

    private static void MoveBullet(Bullet bullet)
    {
        var angle = bullet.Rotation / 16 * 2 * Math.PI;
        bullet.Position = new FloatPosition(
            X: bullet.Position.X + Math.Sin(angle) * 3,
            Y: bullet.Position.Y - Math.Cos(angle) * 3
        );
    }

    public IEnumerable<Bullet> GetAll() => _bullets;
}
