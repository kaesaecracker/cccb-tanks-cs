using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TanksServer.TickSteps;

namespace TanksServer.Services;

internal sealed class GameTickWorker(
    IEnumerable<ITickStep> steps,
    IHostApplicationLifetime lifetime,
    ILogger<GameTickWorker> logger
) : IHostedService, IDisposable
{
    private const int TicksPerSecond = 25;
    private static readonly TimeSpan TickPacing = TimeSpan.FromMilliseconds((int)(1000 / TicksPerSecond));
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

                var wantedDelay = TickPacing - sw.Elapsed;
                if (wantedDelay.Ticks > 0)
                    await Task.Delay(wantedDelay);
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