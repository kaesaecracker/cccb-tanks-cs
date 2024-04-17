using System.Diagnostics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using TanksServer.GameLogic;

namespace TanksServer.Graphics;

internal sealed class DrawPowerUpsStep : IDrawStep
{
    private readonly MapEntityManager _entityManager;
    private readonly bool?[,] _explosiveSprite;

    public DrawPowerUpsStep(MapEntityManager entityManager)
    {
        _entityManager = entityManager;

        using var tankImage = Image.Load<Rgba32>("assets/powerup_explosive.png");
        Debug.Assert(tankImage.Width == tankImage.Height && tankImage.Width == MapService.TileSize);
        _explosiveSprite = new bool?[tankImage.Width, tankImage.Height];

        var whitePixel = new Rgba32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
        for (var y = 0; y < tankImage.Height; y++)
        for (var x = 0; x < tankImage.Width; x++)
        {
            var pixelValue = tankImage[x, y];
            _explosiveSprite[x, y] = pixelValue.A == 0
                ? null
                : pixelValue == whitePixel;
        }
    }

    public void Draw(GamePixelGrid pixels)
    {
        foreach (var powerUp in _entityManager.PowerUps)
        {
            var position = powerUp.Bounds.TopLeft;

            for (byte dy = 0; dy < MapService.TileSize; dy++)
            for (byte dx = 0; dx < MapService.TileSize; dx++)
            {
                var pixelState = _explosiveSprite[dx, dy];
                if (!pixelState.HasValue)
                    continue;

                var (x, y) = position.GetPixelRelative(dx, dy);
                pixels[x, y].EntityType = pixelState.Value
                    ? GamePixelEntityType.PowerUp
                    : null;
            }
        }
    }
}
