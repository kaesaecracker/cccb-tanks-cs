namespace TanksServer.Models;

public class HostConfiguration
{
    public bool EnableServicePointDisplay { get; set; } = true;

    public int ServicePointDisplayMinFrameTimeMs { get; set; } = 25;

    public int ClientDisplayMinFrameTimeMs { get; set; } = 25;
}
