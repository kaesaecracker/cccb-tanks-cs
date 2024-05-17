using ServicePoint2;

namespace TanksServer.Graphics;

internal interface IFrameConsumer
{
    Task OnFrameDoneAsync(GamePixelGrid gamePixelGrid, PixelGrid observerPixels);
}
