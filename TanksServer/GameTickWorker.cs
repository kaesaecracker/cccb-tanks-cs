using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TanksServer.TickSteps;

namespace TanksServer;

internal sealed class GameTickWorker(
    IEnumerable<ITickStep> steps, IHostApplicationLifetime lifetime, ILogger<GameTickWorker> logger
) : IHostedService, IDisposable
{
    private readonly CancellationTokenSource _cancellation = new();
    private readonly List<ITickStep> _steps = steps.ToList();
    private Task? _run;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _run = RunAsync();
        return Task.CompletedTask;
    }

    private async Task RunAsync()
    {
        try
        {
            var sw = new Stopwatch();
            while (!_cancellation.IsCancellationRequested)
            {
                logger.LogTrace("since last frame: {}", sw.Elapsed);
                sw.Restart();

                foreach (var step in _steps)
                    await step.TickAsync();
                
                await Task.Delay(TimeSpan.FromMilliseconds(1000 / 25) - sw.Elapsed);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "game tick service crashed");
            lifetime.StopApplication();
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _cancellation.CancelAsync();
        if (_run != null) await _run;
    }

    public void Dispose()
    {
        _cancellation.Dispose();
        _run?.Dispose();
    }
}