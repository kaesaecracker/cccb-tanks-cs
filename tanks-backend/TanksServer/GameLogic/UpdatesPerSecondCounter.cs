using System.Diagnostics;
using System.Globalization;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace TanksServer.GameLogic;

internal sealed class UpdatesPerSecondCounter(
    ILogger<UpdatesPerSecondCounter> logger
) : ITickStep, IHealthCheck
{
    private static readonly TimeSpan LongTime = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan CriticalUpdateTime = TimeSpan.FromMilliseconds(50);

    private readonly Stopwatch _long = Stopwatch.StartNew();

    private readonly record struct Statistics(
        ulong Updates,
        TimeSpan TotalTime,
        double AverageUpdatesPerSecond,
        TimeSpan MinFrameTime,
        TimeSpan AverageFrameTime,
        TimeSpan MaxFrameTime)
    {
        public override string ToString() =>
            $"{nameof(Updates)}: {Updates}, {nameof(TotalTime)}: {TotalTime}, {nameof(AverageUpdatesPerSecond)}: {AverageUpdatesPerSecond}, {nameof(MinFrameTime)}: {MinFrameTime}, {nameof(AverageFrameTime)}: {AverageFrameTime}, {nameof(MaxFrameTime)}: {MaxFrameTime}";

        public Dictionary<string, object> ToDictionary() => new()
        {
            [nameof(Updates)] = Updates.ToString(),
            [nameof(TotalTime)] = TotalTime.ToString(),
            [nameof(AverageUpdatesPerSecond)] = AverageUpdatesPerSecond.ToString(CultureInfo.InvariantCulture),
            [nameof(MinFrameTime)] = MinFrameTime.ToString(),
            [nameof(AverageFrameTime)] = AverageFrameTime.ToString(),
            [nameof(MaxFrameTime)] = MaxFrameTime.ToString()
        };
    };

    private Statistics? _currentStatistics = null;

    private ulong _updatesSinceLongReset;
    private TimeSpan _minFrameTime = TimeSpan.MaxValue;
    private TimeSpan _maxFrameTime = TimeSpan.MinValue;

    public ValueTask TickAsync(TimeSpan delta)
    {
        if (logger.IsEnabled(LogLevel.Trace))
            logger.LogTrace("time since last update: {}", delta);

        if (delta > CriticalUpdateTime)
        {
            logger.LogCritical("a single update took {}, which is longer than the allowed {}",
                delta, CriticalUpdateTime);
        }

        if (_minFrameTime > delta)
            _minFrameTime = delta;
        if (_maxFrameTime < delta)
            _maxFrameTime = delta;

        _updatesSinceLongReset++;

        if (_long.Elapsed < LongTime)
            return ValueTask.CompletedTask;

        LogCounters();
        ResetCounters();
        return ValueTask.CompletedTask;
    }

    private void LogCounters()
    {
        var time = _long.Elapsed;

        _currentStatistics = new Statistics(
            _updatesSinceLongReset,
            time,
            _updatesSinceLongReset / time.TotalSeconds,
            _minFrameTime,
            time / _updatesSinceLongReset,
            _maxFrameTime);

        if (!logger.IsEnabled(LogLevel.Debug))
            return;

        logger.LogDebug("statistics: {}", _currentStatistics);
    }

    private void ResetCounters()
    {
        _long.Restart();
        _updatesSinceLongReset = 0;
        _minFrameTime = TimeSpan.MaxValue;
        _maxFrameTime = TimeSpan.MinValue;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
        CancellationToken cancellationToken = new())
    {
        var stats = _currentStatistics;
        if (stats == null)
        {
            return Task.FromResult(
                HealthCheckResult.Degraded("no statistics available yet - this is expected shortly after start"));
        }

        if (stats.Value.MaxFrameTime > CriticalUpdateTime)
        {
            return Task.FromResult(HealthCheckResult.Degraded("max frame time too high", null,
                stats.Value.ToDictionary()));
        }

        return Task.FromResult(HealthCheckResult.Healthy("", stats.Value.ToDictionary()));
    }
}
