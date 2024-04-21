using System.Diagnostics;

namespace TanksServer.Models;

[DebuggerDisplay("{TopLeft}, {BottomRight}")]
internal record struct PixelBounds(PixelPosition TopLeft, PixelPosition BottomRight);
