using System.Text;

namespace TanksServer.ServicePointDisplay;

internal sealed class FixedSizeCharGridView(Memory<byte> data, ushort rowLength, ushort rowCount)
{
    public char this[int x, int y]
    {
        get => (char)data.Span[x + y * rowLength];
        set => data.Span[x + y * rowLength] = CharToByte(value);
    }

    public string this[int row]
    {
        get
        {
            var rowStart = row * rowLength;
            return Encoding.UTF8.GetString(data[rowStart..(rowStart + rowLength)].Span);
        }
        set
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(row, rowCount, nameof(row));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(value.Length, rowLength, nameof(value));
            var x = 0;
            for (; x < value.Length; x++)
                this[x, row] = value[x];
            for (; x < rowLength; x++)
                this[x, row] = ' ';
        }
    }

    private static byte CharToByte(char c)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(c);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(c, (char)byte.MaxValue, nameof(c));
        // c# strings are UTF-16
        return (byte)c;
    }
}