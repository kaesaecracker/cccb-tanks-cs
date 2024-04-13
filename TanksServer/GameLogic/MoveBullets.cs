namespace TanksServer.GameLogic;

internal sealed class MoveBullets(BulletManager bullets, IOptions<TanksConfiguration> config) : ITickStep
{
    public Task TickAsync()
    {
        foreach (var bullet in bullets.GetAll())
            MoveBullet(bullet);

        return Task.CompletedTask;
    }

    private void MoveBullet(Bullet bullet)
    {
        var angle = bullet.Rotation * 2 * Math.PI;
        bullet.Position = new FloatPosition(
            bullet.Position.X + Math.Sin(angle) * config.Value.BulletSpeed,
            bullet.Position.Y - Math.Cos(angle) * config.Value.BulletSpeed
        );
    }
}
