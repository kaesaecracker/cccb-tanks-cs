using Microsoft.Extensions.Hosting;

namespace TanksServer.Interactivity;

internal abstract class WebsocketServer<T>(
    ILogger logger
) : IHostedLifecycleService, IDisposable
    where T : WebsocketServerConnection
{
    private readonly SemaphoreSlim _mutex = new(1, 1);
    private bool _closing;
    private readonly HashSet<T> _connections = [];

    public async Task StoppingAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("closing connections");
        await LockedAsync(async () =>
        {
            _closing = true;
            await Task.WhenAll(_connections.Select(c => c.CloseAsync()));
        }, cancellationToken);
        logger.LogInformation("closed connections");
    }

    protected ValueTask ParallelForEachConnectionAsync(Func<T, Task> body) =>
        LockedAsync(async () => await Task.WhenAll(_connections.Select(body)), CancellationToken.None);

    private ValueTask AddConnectionAsync(T connection) => LockedAsync(async () =>
    {
        if (_closing)
        {
            logger.LogWarning("refusing connection because server is shutting down");
            await connection.CloseAsync();
        }

        _connections.Add(connection);
    }, CancellationToken.None);

    private ValueTask RemoveConnectionAsync(T connection) => LockedAsync(() =>
    {
        _connections.Remove(connection);
        return ValueTask.CompletedTask;
    }, CancellationToken.None);

    protected async Task HandleClientAsync(T connection)
    {
        await AddConnectionAsync(connection);
        await connection.ReceiveAsync();
        await RemoveConnectionAsync(connection);
    }

    private async ValueTask LockedAsync(Func<ValueTask> action, CancellationToken cancellationToken)
    {
        await _mutex.WaitAsync(cancellationToken);
        try
        {
            await action();
        }
        finally
        {
            _mutex.Release();
        }
    }

    public void Dispose() => _mutex.Dispose();

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StartedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StartingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
