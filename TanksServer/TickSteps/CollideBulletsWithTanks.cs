namespace TanksServer.TickSteps;

internal sealed class CollideBulletsWithTanks(BulletManager bullets) : ITickStep
{
    public Task TickAsync()
    {
        bullets.RemoveWhere(BulletHitsTank);
        return Task.CompletedTask;
    }

    private bool BulletHitsTank(Bullet bullet)
    {
        return false; // TODO
    }
}
