namespace TanksServer.ServicePointDisplay;

internal sealed class ServicePointDisplayConfiguration
{
    public bool Enable { get; set; } = true;
    public string Hostname { get; set; } = string.Empty;
    public int Port { get; set; }
}
