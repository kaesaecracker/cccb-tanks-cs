namespace DisplayCommands;

public interface IDisplayConnection
{
    ValueTask SendClearAsync();
    
    ValueTask SendCp437DataAsync(ushort x, ushort y, Cp437Grid grid);

    ValueTask SendCharBrightnessAsync(ushort x, ushort y, ByteGrid luma);
    ValueTask SendBrightnessAsync(byte brightness);
    ValueTask SendHardResetAsync();
    ValueTask SendFadeOutAsync(byte loops);
}