namespace TanksServer.Models;

internal interface IMapEntity
{
    FloatPosition Position { get; }

    PixelBounds Bounds { get; }
}
