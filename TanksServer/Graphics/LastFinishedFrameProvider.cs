using DisplayCommands;

namespace TanksServer.Graphics;

internal sealed class LastFinishedFrameProvider
{
    private PixelGrid? _lastFrame;

    public PixelGrid LastFrame
    {
        get => _lastFrame ?? throw new InvalidOperationException("first frame not yet drawn");
        set => _lastFrame = value;
    }
}
