namespace TanksServer.Services;

internal sealed class BulletManager
{
    private readonly HashSet<Bullet> _bullets = new();

    public void Spawn(Bullet bullet) => _bullets.Add(bullet);

    public IEnumerable<Bullet> GetAll() => _bullets;

    public void RemoveWhere(Predicate<Bullet> predicate)
    {
        _bullets.RemoveWhere(predicate);
    }
}
