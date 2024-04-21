namespace DisplayCommands.Internals;

internal enum DisplaySubCommand : ushort
{
    BitmapNormal = 0x0,
    BitmapCompressZ = 0x677a,
    BitmapCompressBz = 0x627a,
    BitmapCompressLz = 0x6c7a,
    BitmapCompressZs = 0x7a73
}
