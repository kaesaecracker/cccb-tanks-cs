using System.Diagnostics;

namespace DisplayCommands;

public sealed class ByteGrid(ushort width, ushort height) : IEquatable<ByteGrid>
{
    public ushort Height { get; } = height;

    public ushort Width { get; } = width;

    internal Memory<byte> Data { get; } = new byte[width * height].AsMemory();

    public byte this[ushort x, ushort y]
    {
        get => Data.Span[GetIndex(x, y)];
        set => Data.Span[GetIndex(x, y)] = value;
    }

    private int GetIndex(ushort x, ushort y)
    {
        Debug.Assert(x < Width);
        Debug.Assert(y < Height);
        return x + y * Width;
    }

    public void Clear() => Data.Span.Clear();

    public bool Equals(ByteGrid? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Height == other.Height && Width == other.Width && Data.Span.SequenceEqual(other.Data.Span);
    }

    public override bool Equals(object? obj) => ReferenceEquals(this, obj) || (obj is ByteGrid other && Equals(other));
    public override int GetHashCode() => HashCode.Combine(Height, Width, Data);
    public static bool operator ==(ByteGrid? left, ByteGrid? right) => Equals(left, right);
    public static bool operator !=(ByteGrid? left, ByteGrid? right) => !Equals(left, right);
}