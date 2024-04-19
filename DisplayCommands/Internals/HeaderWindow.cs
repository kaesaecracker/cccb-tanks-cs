using System.Buffers.Binary;
using System.Runtime.InteropServices;

namespace DisplayCommands.Internals;

[StructLayout(LayoutKind.Sequential, Pack = 16, Size = 10)]
internal struct HeaderWindow
{
    public DisplayCommand Command;

    public ushort PosX;

    public ushort PosY;

    public ushort Width;

    public ushort Height;

    public void ChangeToNetworkOrder()
    {
        if (!BitConverter.IsLittleEndian)
            return;
        Command = (DisplayCommand)BinaryPrimitives.ReverseEndianness((ushort)Command);
        PosX = BinaryPrimitives.ReverseEndianness(PosX);
        PosY = BinaryPrimitives.ReverseEndianness(PosY);
        Width = BinaryPrimitives.ReverseEndianness(Width);
        Height = BinaryPrimitives.ReverseEndianness(Height);
    }
}
