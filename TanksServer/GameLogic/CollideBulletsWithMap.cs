namespace TanksServer.GameLogic;

internal sealed class CollideBulletsWithMap(BulletManager bullets, MapService map) : ITickStep
{
    public Task TickAsync(TimeSpan _)
    {
        bullets.RemoveWhere(TryHitAndDestroyWall);
        return Task.CompletedTask;
    }

    private bool TryHitAndDestroyWall(Bullet bullet)
    {
        var pixel = bullet.Position.ToPixelPosition();
        if (!map.Current.IsWall(pixel))
            return false;

        map.Current.DestroyWallAt(pixel);
        return true;
    }
}
