using System.Diagnostics;
using Microsoft.Extensions.Hosting;

namespace TanksServer.GameLogic;

internal sealed class GameTickWorker(
    IEnumerable<ITickStep> steps,
    IHostApplicationLifetime lifetime,
    ILogger<GameTickWorker> logger
) : IHostedLifecycleService, IDisposable
{
    private readonly CancellationTokenSource _cancellation = new();
    private readonly TaskCompletionSource _shutdownCompletion = new();
    private readonly List<ITickStep> _steps = steps.ToList();

    public async Task StartedAsync(CancellationToken cancellationToken)
    {
        await Task.Yield();

        // the first tick is really short (< 0.01ms) if this line is directly above the while
        var sw = Stopwatch.StartNew();
        await Task.Delay(1, CancellationToken.None).ConfigureAwait(false);

        try
        {
            while (!_cancellation.IsCancellationRequested)
            {
                var delta = sw.Elapsed;
                sw.Restart();

                foreach (var step in _steps)
                    await step.TickAsync(delta);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "game tick service crashed");
            lifetime.StopApplication();
        }

        _shutdownCompletion.SetResult();
    }

    public Task StoppingAsync(CancellationToken cancellationToken) => _cancellation.CancelAsync();

    public Task StopAsync(CancellationToken cancellationToken) => _shutdownCompletion.Task;

    public void Dispose() => _cancellation.Dispose();

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StartingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
