namespace TanksServer.GameLogic;

internal sealed class MoveBullets(BulletManager bullets, IOptions<TanksConfiguration> config) : ITickStep
{
    public Task TickAsync(TimeSpan delta)
    {
        foreach (var bullet in bullets.GetAll())
            MoveBullet(bullet, delta);

        return Task.CompletedTask;
    }

    private void MoveBullet(Bullet bullet, TimeSpan delta)
    {
        var speed = config.Value.BulletSpeed * delta.TotalSeconds;
        var angle = bullet.Rotation * 2 * Math.PI;
        bullet.Position = new FloatPosition(
            bullet.Position.X + Math.Sin(angle) * speed,
            bullet.Position.Y - Math.Cos(angle) * speed
        );
    }
}
