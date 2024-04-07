namespace TanksServer.TickSteps;

internal sealed class MoveBullets(BulletManager bullets) : ITickStep
{
    public Task TickAsync()
    {
        foreach (var bullet in bullets.GetAll())
            MoveBullet(bullet);

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
}
