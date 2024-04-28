using DisplayCommands;

namespace TanksServer.Graphics;

internal interface IFrameConsumer
{
    ValueTask OnFrameDoneAsync(GamePixelGrid gamePixelGrid, PixelGrid observerPixels);
}
