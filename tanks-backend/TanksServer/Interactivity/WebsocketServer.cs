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

    private Task AddConnectionAsync(T connection) => Locked(() =>
    {
        if (_closing)
        {
            logger.LogWarning("refusing connection because server is shutting down");
            return connection.CloseAsync();
        }

        _connections.Add(connection);
        return Task.CompletedTask;
    }, CancellationToken.None);

    private Task RemoveConnectionAsync(T connection) => Locked(() =>
    {
        _connections.Remove(connection);
        return Task.CompletedTask;
    }, CancellationToken.None);

    protected async Task HandleClientAsync(T connection)
    {
        await AddConnectionAsync(connection);
        await connection.ReceiveAsync();
        await RemoveConnectionAsync(connection);
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
