using TanksServer.Helpers;
using TanksServer.Services;

namespace TanksServer.ServicePointDisplay;

internal sealed class PixelDisplayBufferView : DisplayBufferView
{
    private PixelDisplayBufferView(byte[] data) : base(data)
    {
        Pixels = new FixedSizeBitFieldView(Data.AsMemory(10));
    }

    // ReSharper disable once CollectionNeverQueried.Global (setting values in collection updates underlying byte array)
    public FixedSizeBitFieldView Pixels { get; }

    public static PixelDisplayBufferView New(ushort x, ushort y, ushort widthInTiles, ushort pixelRows)
    {
        // 10 bytes header, one byte per tile row (with one bit each pixel) after that
        var size = 10 + widthInTiles * pixelRows;
        return new PixelDisplayBufferView(new byte[size])
        {
            Mode = 19,
            TileX = x,
            TileY = y,
            WidthInTiles = widthInTiles,
            RowCount = pixelRows
        };
    }
}