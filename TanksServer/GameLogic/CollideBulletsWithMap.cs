namespace TanksServer.GameLogic;

internal sealed class CollideBulletsWithMap(
    BulletManager bullets,
    MapService map,
    IOptions<GameRulesConfiguration> options
) : ITickStep
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

        if (options.Value.DestructibleWalls)
            map.Current.DestroyWallAt(pixel);
        return true;
    }
}
