using TanksServer.ServicePointDisplay;

namespace TanksServer.Graphics;

internal sealed class LastFinishedFrameProvider
{
    private PixelDisplayBufferView? _lastFrame;
    
    public PixelDisplayBufferView LastFrame
    {
        get => _lastFrame ?? throw new InvalidOperationException("first frame not yet drawn");
        set => _lastFrame = value;
    }
}
