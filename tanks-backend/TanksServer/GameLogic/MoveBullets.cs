namespace TanksServer.GameLogic;

internal sealed class MoveBullets(MapEntityManager entityManager) : ITickStep
{
    public ValueTask TickAsync(TimeSpan delta)
    {
        foreach (var bullet in entityManager.Bullets)
            MoveBullet(bullet, delta);

        return ValueTask.CompletedTask;
    }

    private static void MoveBullet(Bullet bullet, TimeSpan delta)
    {
        var speed = bullet.Speed * delta.TotalSeconds;
        var angle = bullet.Rotation * 2 * Math.PI;
        bullet.Position = new FloatPosition(
            bullet.Position.X + Math.Sin(angle) * speed,
            bullet.Position.Y - Math.Cos(angle) * speed
        );
    }
}
