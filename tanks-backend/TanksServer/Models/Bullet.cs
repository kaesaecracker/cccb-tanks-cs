namespace TanksServer.Models;

internal sealed class Bullet : IMapEntity
{
    public required Player Owner { get; init; }

    public required double Rotation { get; set; }

    public required FloatPosition Position { get; set; }

    public required bool IsExplosive { get; init; }

    public required DateTime Timeout { get; init; }

    public PixelBounds Bounds => new(Position.ToPixelPosition(), Position.ToPixelPosition());

    internal required DateTime OwnerCollisionAfter { get; init; }

    public required double Speed { get; set; }

    public required double Acceleration { get; init; }

    public required bool IsSmart { get; init; }
}
