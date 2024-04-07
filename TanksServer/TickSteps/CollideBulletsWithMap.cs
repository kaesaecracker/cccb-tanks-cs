using TanksServer.Helpers;
using TanksServer.Models;
using TanksServer.Services;

namespace TanksServer.TickSteps;

internal sealed class CollideBulletsWithMap(BulletManager bullets, MapService map) : ITickStep
{
    public Task TickAsync()
    {
        bullets.RemoveWhere(BulletHitsWall);
        return Task.CompletedTask;
    }

    private bool BulletHitsWall(Bullet bullet)
    {
        return map.IsCurrentlyWall(bullet.Position.ToPixelPosition().ToTilePosition());
    }
}
