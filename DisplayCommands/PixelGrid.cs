using System.Diagnostics;

namespace DisplayCommands;

public sealed class PixelGrid(ushort width, ushort height)
{
    private readonly ByteGrid _byteGrid = new((ushort)(width / 8u), height);

    public ushort Width { get; } = width;

    public ushort Height { get; } = height;

    public Memory<byte> Data => _byteGrid.Data;

    public bool this[ushort x, ushort y]
    {
        get
        {
            Debug.Assert(y < Height);
            var (byteIndex, bitInByteMask) = GetIndexes(x);
            var byteVal = _byteGrid[byteIndex, y];
            return (byteVal & bitInByteMask) != 0;
        }
        set
        {
            Debug.Assert(y < Height);
            var (byteIndex, bitInByteMask) = GetIndexes(x);
            if (value)
                _byteGrid[byteIndex, y] |= bitInByteMask;
            else
                _byteGrid[byteIndex, y] &= (byte)(ushort.MaxValue ^ bitInByteMask);
        }
    }

    public void Clear() => _byteGrid.Clear();

    private (ushort byteIndex, byte bitInByteMask) GetIndexes(int x)
    {
        Debug.Assert(x < Width);
        var byteIndex = (ushort)(x / 8);
        Debug.Assert(byteIndex < Width);
        var bitInByteIndex = (byte)(7 - x % 8);
        Debug.Assert(bitInByteIndex < 8);
        var bitInByteMask = (byte)(1 << bitInByteIndex);
        return (byteIndex, bitInByteMask);
    }
}