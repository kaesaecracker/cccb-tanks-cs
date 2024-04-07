namespace TanksServer.Helpers;

internal sealed class DisplayPixelBuffer(byte[] data)
{
    public byte[] Data => data;

    public byte Magic1
    {
        get => data[0];
        set => data[0] = value;
    }

    public byte Magic2
    {
        get => data[1];
        set => data[1] = value;
    }

    public ushort X
    {
        get => GetTwoBytes(2);
        set => SetTwoBytes(2, value);
    }

    public ushort Y
    {
        get => GetTwoBytes(4);
        set => SetTwoBytes(4, value);
    }

    public ushort WidthInTiles
    {
        get => GetTwoBytes(6);
        set => SetTwoBytes(6, value);
    }

    public ushort HeightInPixels
    {
        get => GetTwoBytes(8);
        set => SetTwoBytes(8, value);
    }

    public FixedSizeBitFieldView Pixels { get; } = new(data.AsMemory(10));

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
