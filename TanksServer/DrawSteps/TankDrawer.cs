using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using TanksServer.Helpers;
using TanksServer.Services;

namespace TanksServer.DrawSteps;

internal sealed class TankDrawer : IDrawStep
{
    private readonly TankManager _tanks;
    private readonly bool[] _tankSprite;
    private readonly int _tankSpriteWidth;

    public TankDrawer(TankManager tanks)
    {
        _tanks = tanks;

        using var tankImage = Image.Load<Rgba32>("assets/tank.png");
        _tankSprite = new bool[tankImage.Height * tankImage.Width];

        var whitePixel = new Rgba32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
        var i = 0;
        for (var y = 0; y < tankImage.Height; y++)
        for (var x = 0; x < tankImage.Width; x++, i++)
        {
            _tankSprite[i] = tankImage[x, y] == whitePixel;
        }

        _tankSpriteWidth = tankImage.Width;
    }

    public void Draw(DisplayPixelBuffer buffer)
    {
        foreach (var tank in _tanks)
        {
            var pos = tank.Position.ToPixelPosition();
            var rotationVariant = (int)Math.Round(tank.Rotation) % 16;
            for (var dy = 0; dy < MapService.TileSize; dy++)
            {
                var rowStartIndex = (pos.Y + dy) * MapService.PixelsPerRow;

                for (var dx = 0; dx < MapService.TileSize; dx++)
                {
                    if (!TankSpriteAt(dx, dy, rotationVariant))
                        continue;

                    var i = rowStartIndex + pos.X + dx;
                    buffer.Pixels[i] = true;
                }
            }
        }
    }

    private bool TankSpriteAt(int dx, int dy, int tankRotation)
    {
        var x = tankRotation % 4 * (MapService.TileSize + 1);
        var y = (int)Math.Floor(tankRotation / 4d) * (MapService.TileSize + 1);
        var index = (y + dy) * _tankSpriteWidth + x + dx;

        return _tankSprite[index];
    }
}
