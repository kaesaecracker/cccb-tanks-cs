using ServicePoint;

namespace TanksServer.Graphics;

internal interface IFrameConsumer
{
    Task OnFrameDoneAsync(GamePixelGrid gamePixelGrid, PixelGrid observerPixels);
}
