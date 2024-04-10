using System.Diagnostics;

namespace TanksServer.ServicePointDisplay;

internal sealed class FixedSizeBitGridView(Memory<byte> data, int columns, int rows)
{
    private readonly FixedSizeBitRowView _bits = new(data);

    public bool this[PixelPosition position]
    {
        get => _bits[ToPixelIndex(position)];
        set => _bits[ToPixelIndex(position)] = value;
    }

    private int ToPixelIndex(PixelPosition position)
    { 
        Debug.Assert(position.X < columns);
        Debug.Assert(position.Y < rows);
        var index = position.Y * columns + position.X;
        ArgumentOutOfRangeException.ThrowIfNegative(index, nameof(position));
        return index;
    }
}