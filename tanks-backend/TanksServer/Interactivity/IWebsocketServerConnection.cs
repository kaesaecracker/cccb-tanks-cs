namespace TanksServer.Interactivity;

internal interface IWebsocketServerConnection
{
    Task CloseAsync();

    Task Done { get; }
}
