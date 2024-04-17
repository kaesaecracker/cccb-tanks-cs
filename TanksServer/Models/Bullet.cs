namespace TanksServer.Models;

internal sealed class Bullet(Player tankOwner, FloatPosition position, double rotation, bool isExplosive) : IMapEntity
{
    public Player Owner { get; } = tankOwner;

    public double Rotation { get; } = rotation;

    public FloatPosition Position { get; set; } = position;

    public bool IsExplosive { get; } = isExplosive;

    public PixelBounds Bounds => new (Position.ToPixelPosition(), Position.ToPixelPosition());
}
