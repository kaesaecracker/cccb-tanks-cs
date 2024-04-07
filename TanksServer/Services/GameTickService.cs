using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TanksServer.TickSteps;

namespace TanksServer.Services;

internal sealed class GameTickService(
    IEnumerable<ITickStep> steps, IHostApplicationLifetime lifetime, ILogger<GameTickService> logger
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
            while (!_cancellation.IsCancellationRequested)
            {
                foreach (var step in _steps)
                    await step.TickAsync();
                await Task.Delay(1000 / 25);
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
