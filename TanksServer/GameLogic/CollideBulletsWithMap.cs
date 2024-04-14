namespace TanksServer.GameLogic;

internal sealed class CollideBulletsWithMap(BulletManager bullets, MapService map) : ITickStep
{
    public Task TickAsync()
    {
        bullets.RemoveWhere(BulletHitsWall);
        return Task.CompletedTask;
    }

    private bool BulletHitsWall(Bullet bullet) =>
        map.Current.IsWall(bullet.Position.ToPixelPosition());
}
