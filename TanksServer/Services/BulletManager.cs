using System.Collections;
using TanksServer.Helpers;
using TanksServer.Models;

namespace TanksServer.Services;

internal sealed class BulletManager(MapService map) : ITickStep
{
    private readonly HashSet<Bullet> _bullets = new();

    public void Spawn(Bullet bullet) => _bullets.Add(bullet);

    public IEnumerable<Bullet> GetAll() => _bullets;

    public Task TickAsync()
    {
        HashSet<Bullet> bulletsToRemove = new();
        foreach (var bullet in _bullets)
        {
            MoveBullet(bullet);

            if (BulletHitsWall(bullet))
                bulletsToRemove.Add(bullet);
        }

        _bullets.RemoveWhere(b => bulletsToRemove.Contains(b));
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

    private bool BulletHitsWall(Bullet bullet)
    {
        return map.IsCurrentlyWall(bullet.Position.ToPixelPosition().ToTilePosition());
    }
}
