using Microsoft.Extensions.Hosting;

namespace TanksServer.Services;

internal sealed class GameTickService(IEnumerable<ITickStep> steps) : IHostedService, IDisposable
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
        while (!_cancellation.IsCancellationRequested)
        {
            foreach (var step in _steps)
                await step.TickAsync();
            await Task.Delay(1000/25);
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

public interface ITickStep
{
    Task TickAsync();
}
