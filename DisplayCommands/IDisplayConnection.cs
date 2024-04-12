namespace DisplayCommands;

public interface IDisplayConnection
{
    ValueTask SendClearAsync();

    ValueTask SendCp437DataAsync(ushort x, ushort y, Cp437Grid grid);

    ValueTask SendCharBrightnessAsync(ushort x, ushort y, ByteGrid luma);

    ValueTask SendBrightnessAsync(byte brightness);

    ValueTask SendHardResetAsync();

    ValueTask SendFadeOutAsync(byte loops);

    public ValueTask SendBitmapLinearWindowAsync(ushort x, ushort y, PixelGrid pixels);

    /// <summary>
    /// Returns the IPv4 address that is associated with the interface with which the display is reachable.
    /// </summary>
    /// <returns>IPv4 as text</returns>
    public string GetLocalIPv4();
}