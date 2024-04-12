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
}