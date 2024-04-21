using System.Runtime.InteropServices;
using EndiannessSourceGenerator;

namespace DisplayCommands.Internals;

[StructEndianness(IsLittleEndian = false)]
[StructLayout(LayoutKind.Sequential, Pack = 16, Size = 10)]
internal partial struct HeaderWindow
{
    private ushort _command;

    private ushort _posX;

    private ushort _posY;

    private ushort _width;

    private ushort _height;
}
