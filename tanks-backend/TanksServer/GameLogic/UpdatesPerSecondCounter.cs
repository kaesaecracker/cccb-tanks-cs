using System.Diagnostics;

namespace TanksServer.GameLogic;

internal sealed class UpdatesPerSecondCounter(ILogger<UpdatesPerSecondCounter> logger) : ITickStep
{
    private readonly Stopwatch _long = Stopwatch.StartNew();
    private ulong _updatesSinceLongReset;
    private TimeSpan _minFrameTime = TimeSpan.MaxValue;
    private TimeSpan _maxFrameTime = TimeSpan.MinValue;

    public ValueTask TickAsync(TimeSpan delta)
    {
        if (logger.IsEnabled(LogLevel.Trace))
            logger.LogTrace("time since last update: {}", delta);
        if (delta.TotalSeconds > 1)
            logger.LogCritical("single update took {}", delta);

        if (_minFrameTime > delta)
            _minFrameTime = delta;
        if (_maxFrameTime < delta)
            _maxFrameTime = delta;

        _updatesSinceLongReset++;

        if (_long.Elapsed.TotalSeconds < 10)
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
        var average = Math.Round(_updatesSinceLongReset / time.TotalSeconds);
        var min = Math.Round(1 / _maxFrameTime.TotalSeconds);
        var max = Math.Round(1 / _minFrameTime.TotalSeconds);
        logger.LogDebug("UPS stats for {} updates in {}: avg={}, min={}, max={}",
            _updatesSinceLongReset, time, average, min, max);
    }

    private void ResetCounters()
    {
        _long.Restart();
        _updatesSinceLongReset = 0;
        _minFrameTime = TimeSpan.MaxValue;
        _maxFrameTime = TimeSpan.MinValue;
    }
}
