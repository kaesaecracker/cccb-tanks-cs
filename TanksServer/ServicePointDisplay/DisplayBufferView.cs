using TanksServer.Models;

namespace TanksServer.ServicePointDisplay;

internal class DisplayBufferView(byte[] data)
{
    public byte[] Data => data;

    public ushort Mode
    {
        get => GetTwoBytes(0);
        set => SetTwoBytes(0, value);
    }

    public ushort TileX
    {
        get => GetTwoBytes(2);
        set => SetTwoBytes(2, value);
    }

    public ushort TileY
    {
        get => GetTwoBytes(4);
        set => SetTwoBytes(4, value);
    }

    public ushort WidthInTiles
    {
        get => GetTwoBytes(6);
        set => SetTwoBytes(6, value);
    }

    public ushort RowCount
    {
        get => GetTwoBytes(8);
        set => SetTwoBytes(8, value);
    }

    public TilePosition Position
    {
        get => new(TileX, TileY);
        set
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThan(value.X, ushort.MaxValue);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(value.Y, ushort.MaxValue);
            ArgumentOutOfRangeException.ThrowIfNegative(value.X);
            ArgumentOutOfRangeException.ThrowIfNegative(value.Y);

            TileX = (ushort)value.X;
            TileY = (ushort)value.Y;
        }
    }

    private ushort GetTwoBytes(int index)
    {
        return (ushort)(data[index] * byte.MaxValue + data[index + 1]);
    }

    private void SetTwoBytes(int index, ushort value)
    {
        data[index] = (byte)(value / byte.MaxValue);
        data[index + 1] = (byte)(value % byte.MaxValue);
    }
}