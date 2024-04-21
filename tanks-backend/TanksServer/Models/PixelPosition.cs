using System.Diagnostics;
using TanksServer.GameLogic;

namespace TanksServer.Models;

[DebuggerDisplay("({X} | {Y})")]
internal readonly struct PixelPosition(int x, int y)
{
    public int X { get; } = (x + MapService.PixelsPerRow) % MapService.PixelsPerRow;
    public int Y { get; } = (y + MapService.PixelsPerColumn) % MapService.PixelsPerColumn;

    public void Deconstruct(out int x, out int y)
    {
        x = X;
        y = Y;
    }
}
