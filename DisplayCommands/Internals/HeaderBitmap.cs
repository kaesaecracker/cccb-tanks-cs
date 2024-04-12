using System.Runtime.InteropServices;

namespace DisplayCommands.Internals;

[StructLayout(LayoutKind.Sequential, Pack = 16, Size = 10)]
internal struct HeaderBitmap
{
    public DisplayCommand Command;

    public ushort Offset;

    public ushort Length;

    public DisplaySubCommand SubCommand;

    public ushort Reserved;
}