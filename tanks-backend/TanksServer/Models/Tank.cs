using System.Diagnostics;
using TanksServer.GameLogic;

namespace TanksServer.Models;

internal sealed class Tank : IMapEntity
{
    private double _rotation;

    public required Player Owner { get; init; }

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

    public bool Moving { get; set; }

    public required FloatPosition Position { get; set; }

    public PixelBounds Bounds => Position.GetBoundsForCenter(MapService.TileSize);

    public int Orientation => (int)Math.Round(Rotation * 16) % 16;

    public required Magazine Magazine { get; set; }
}
