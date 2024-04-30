using System.Diagnostics;
using Microsoft.Extensions.Hosting;

namespace TanksServer.GameLogic;

internal sealed class GameTickWorker(
    IEnumerable<ITickStep> steps,
    IHostApplicationLifetime lifetime,
    ILogger<GameTickWorker> logger
) : IHostedService, IDisposable
{
    private readonly CancellationTokenSource _cancellation = new();
    private readonly List<ITickStep> _steps = steps.ToList();
    private Task? _run;

    public void Dispose()
    {
        _cancellation.Dispose();
        _run?.Dispose();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _run = RunAsync();
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _cancellation.CancelAsync();
        if (_run != null) await _run;
    }

    private async Task RunAsync()
    {
        // do not block in StartAsync
        await Task.Delay(1).ConfigureAwait(false);

        try
        {
            var sw = new Stopwatch();
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
    }
}
