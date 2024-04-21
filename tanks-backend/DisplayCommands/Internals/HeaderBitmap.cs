using System.Runtime.InteropServices;
using EndiannessSourceGenerator;

namespace DisplayCommands.Internals;

[StructEndianness(IsLittleEndian = false)]
[StructLayout(LayoutKind.Sequential, Pack = 16, Size = 10)]
internal partial struct HeaderBitmap
{
    private ushort _command;

    private ushort _offset;

    private ushort _length;

    private ushort _subCommand;

    private ushort _reserved;
}
