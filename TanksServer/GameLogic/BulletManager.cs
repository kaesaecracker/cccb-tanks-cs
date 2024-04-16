namespace TanksServer.GameLogic;

internal sealed class BulletManager
{
    private readonly HashSet<Bullet> _bullets = [];

    public void Spawn(Player tankOwner, FloatPosition position, double rotation)
        => _bullets.Add(new Bullet(tankOwner, position, rotation));

    public IEnumerable<Bullet> GetAll() => _bullets;

    public void RemoveWhere(Predicate<Bullet> predicate) => _bullets.RemoveWhere(predicate);
}
