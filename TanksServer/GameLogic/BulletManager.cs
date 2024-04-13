namespace TanksServer.GameLogic;

internal sealed class BulletManager
{
    private readonly HashSet<Bullet> _bullets = [];

    public void Spawn(Bullet bullet) => _bullets.Add(bullet);

    public IEnumerable<Bullet> GetAll() => _bullets;

    public void RemoveWhere(Predicate<Bullet> predicate) => _bullets.RemoveWhere(predicate);
}
