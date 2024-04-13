namespace TanksServer.Models;

internal sealed class Bullet(Player tankOwner, FloatPosition position, double rotation) : IMapEntity
{
    public Player Owner { get; } = tankOwner;

    public double Rotation { get; set; } = rotation;

    public FloatPosition Position { get; set; } = position;

    public PixelBounds Bounds => new (Position.ToPixelPosition(), Position.ToPixelPosition());
}
