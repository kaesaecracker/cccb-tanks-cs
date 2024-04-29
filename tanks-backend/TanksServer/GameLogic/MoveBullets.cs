namespace TanksServer.GameLogic;

internal sealed class MoveBullets(
    MapEntityManager entityManager,
    IOptions<GameRules> options
) : ITickStep
{
    private readonly double _smartBulletInertia = options.Value.SmartBulletInertia;

    public ValueTask TickAsync(TimeSpan delta)
    {
        foreach (var bullet in entityManager.Bullets)
            MoveBullet(bullet, delta);

        return ValueTask.CompletedTask;
    }

    private void MoveBullet(Bullet bullet, TimeSpan delta)
    {
        if (bullet.IsSmart && TryGetSmartRotation(bullet.Position, bullet.Owner, out var wantedRotation))
        {
            var inertiaFactor = _smartBulletInertia * delta.TotalSeconds;
            var difference = wantedRotation - bullet.Rotation;
            bullet.Rotation += difference * inertiaFactor;
        }

        var speed = bullet.Speed * delta.TotalSeconds;
        var angle = bullet.Rotation * 2 * Math.PI;
        bullet.Position = new FloatPosition(
            bullet.Position.X + Math.Sin(angle) * speed,
            bullet.Position.Y - Math.Cos(angle) * speed
        );
    }

    private bool TryGetSmartRotation(FloatPosition position, Player bulletOwner, out double rotation)
    {
        var nearestEnemy = entityManager.Tanks
            .Where(t => t.Owner != bulletOwner)
            .MinBy(t => position.Distance(t.Position));

        if (nearestEnemy == null)
        {
            rotation = double.NaN;
            return false;
        }

        var rotationRadians = Math.Atan2(
            y: nearestEnemy.Position.Y - position.Y,
            x: nearestEnemy.Position.X - position.X
        ) + (Math.PI / 2);
        rotation = rotationRadians / (2 * Math.PI);
        return true;
    }
}
