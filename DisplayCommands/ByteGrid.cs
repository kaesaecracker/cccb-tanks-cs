using System.Diagnostics;

namespace DisplayCommands;

public class ByteGrid(ushort width, ushort height)
{
    public ushort Height { get; } = height;
    
    public ushort Width { get; } = width;
    
    internal Memory<byte> Data { get; } = new byte[width * height].AsMemory();

    public byte this[ushort x, ushort y]
    {
        get => Data.Span[ GetIndex(x, y)];
        set => Data.Span[GetIndex(x, y)] = value;
    }

    private int GetIndex(ushort x, ushort y)
    {
        Debug.Assert(x < Width);
        Debug.Assert(y < Height);
        return x + y * Width;
    }

    public void Clear() => Data.Span.Clear();
}
