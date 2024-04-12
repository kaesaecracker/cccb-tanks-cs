using System.Diagnostics;
using System.Text;

namespace DisplayCommands;

public sealed class Cp437Grid(ushort width, ushort height)
{
    private readonly ByteGrid _byteGrid = new(width, height);
    
    public ushort Height { get; } = height;
    
    public ushort Width { get; } = width;

    internal Memory<byte> Data => _byteGrid.Data;
    
    private readonly Encoding _encoding = Encoding.GetEncoding(437);

    public char this[ushort x, ushort y]
    {
        get => ByteToChar(_byteGrid[x, y]);
        set => _byteGrid[x, y] = CharToByte(value);
    }

    public string this[ushort row]
    {
        get
        {
            var rowStart = row * Width;
            return _encoding.GetString(_byteGrid.Data[rowStart..(rowStart + Width)].Span);
        }
        set
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(row, Height, nameof(row));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(value.Length, Width, nameof(value));
            ushort x = 0;
            for (; x < value.Length; x++)
                _byteGrid[x, row] = CharToByte(value[x]);
            for (; x < Width; x++)
                _byteGrid[x, row] = CharToByte(' ');
        }
    }

    private byte CharToByte(char c)
    {
        ReadOnlySpan<char> valuesStr = stackalloc char[] { c };
        Span<byte> convertedStr = stackalloc byte[1];
        var consumed = _encoding.GetBytes(valuesStr, convertedStr);
        Debug.Assert(consumed == 1);
        return convertedStr[0];
        
    }

    private char ByteToChar(byte b)
    {
        ReadOnlySpan<byte> valueBytes = stackalloc byte[] { b };
        Span<char> resultStr = stackalloc char[1];
        _encoding.GetChars(valueBytes, resultStr);
        return resultStr[0];
    }
}