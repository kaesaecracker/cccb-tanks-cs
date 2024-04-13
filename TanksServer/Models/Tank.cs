using System.Diagnostics;
using TanksServer.GameLogic;

namespace TanksServer.Models;

internal sealed class Tank(Player player, FloatPosition spawnPosition) : IMapEntity
{
    private double _rotation;

    public Player Owner { get; } = player;

    public double Rotation
    {
        get => _rotation;
        set
        {
            var newRotation = (value % 1d + 1d) % 1d;
            Debug.Assert(newRotation is >= 0 and < 1);
            _rotation = newRotation;
        }
    }

    public DateTime NextShotAfter { get; set; }

    public bool Moved { get; set; }

    public FloatPosition Position { get; set; } = spawnPosition;

    public PixelBounds Bounds => GetBoundsForCenter(Position);

    public static PixelBounds GetBoundsForCenter(FloatPosition position)
    {
        var pixelPosition = position.ToPixelPosition();
        return new PixelBounds(
            pixelPosition.GetPixelRelative(-3, -3),
            pixelPosition.GetPixelRelative(4, 4)
        );
    }
}
