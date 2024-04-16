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
        try
        {
            var sw = new Stopwatch();
            while (!_cancellation.IsCancellationRequested)
            {
                logger.LogTrace("since last frame: {}", sw.Elapsed);

                var delta = sw.Elapsed;
                sw.Restart();

                foreach (var step in _steps)
                    await step.TickAsync(delta);

                await Task.Delay(1);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "game tick service crashed");
            lifetime.StopApplication();
        }
    }
}
