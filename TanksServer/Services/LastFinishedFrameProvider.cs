using TanksServer.Helpers;

namespace TanksServer.Services;

internal sealed class LastFinishedFrameProvider
{
    private DisplayPixelBuffer? _lastFrame;
    
    public DisplayPixelBuffer LastFrame
    {
        get => _lastFrame ?? throw new InvalidOperationException("first frame not yet drawn");
        set => _lastFrame = value;
    }
}
