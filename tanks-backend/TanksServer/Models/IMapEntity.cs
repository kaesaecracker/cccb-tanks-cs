namespace TanksServer.Models;

internal interface IMapEntity
{
    FloatPosition Position { get; set; }

    PixelBounds Bounds { get; }
}
