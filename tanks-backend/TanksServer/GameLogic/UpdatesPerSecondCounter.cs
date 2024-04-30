using System.Diagnostics;

namespace TanksServer.GameLogic;

internal sealed class UpdatesPerSecondCounter(ILogger<UpdatesPerSecondCounter> logger) : ITickStep
{
    private static readonly TimeSpan LongTime = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan CriticalUpdateTime = TimeSpan.FromMilliseconds(50);

    private readonly Stopwatch _long = Stopwatch.StartNew();
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
        if (!logger.IsEnabled(LogLevel.Debug))
            return;

        var time = _long.Elapsed;
        var averageTime = Math.Round(time.TotalMilliseconds / _updatesSinceLongReset, 2);
        var averageUps = Math.Round(_updatesSinceLongReset / time.TotalSeconds, 2);
        var min = Math.Round(_minFrameTime.TotalMilliseconds, 2);
        var max = Math.Round(_maxFrameTime.TotalMilliseconds, 2);
        logger.LogDebug("count={}, time={}, avg={}ms, ups={}, min={}ms, max={}ms",
            _updatesSinceLongReset, time, averageTime, averageUps, min, max);
    }

    private void ResetCounters()
    {
        _long.Restart();
        _updatesSinceLongReset = 0;
        _minFrameTime = TimeSpan.MaxValue;
        _maxFrameTime = TimeSpan.MinValue;
    }
}
