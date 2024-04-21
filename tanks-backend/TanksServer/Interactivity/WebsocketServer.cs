using Microsoft.Extensions.Hosting;

namespace TanksServer.Interactivity;

internal class WebsocketServer<T>(
    ILogger logger
) : IHostedLifecycleService, IDisposable
    where T : IWebsocketServerConnection
{
    private readonly SemaphoreSlim _mutex = new(1, 1);
    private bool _closing;
    private readonly HashSet<T> _connections = [];

    public async Task StoppingAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("closing connections");
        await Locked(async () =>
        {
            _closing = true;
            await Task.WhenAll(_connections.Select(c => c.CloseAsync()));
        }, cancellationToken);
        logger.LogInformation("closed connections");
    }

    protected Task ParallelForEachConnectionAsync(Func<T, Task> body)
    {
        _mutex.Wait();
        try
        {
            return Task.WhenAll(_connections.Select(body));
        }
        finally
        {
            _mutex.Release();
        }
    }

    protected Task AddConnection(T connection) => Locked(() =>
    {
        if (_closing)
        {
            logger.LogWarning("refusing connection because server is shutting down");
            return connection.CloseAsync();
        }

        _connections.Add(connection);
        return Task.CompletedTask;
    }, CancellationToken.None);

    protected Task RemoveConnection(T connection) => Locked(() =>
    {
        _connections.Remove(connection);
        return Task.CompletedTask;
    }, CancellationToken.None);

    protected async Task HandleClientAsync(T connection)
    {
        await AddConnection(connection);
        await connection.Done;
        await RemoveConnection(connection);
    }

    private async Task Locked(Func<Task> action, CancellationToken cancellationToken)
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
