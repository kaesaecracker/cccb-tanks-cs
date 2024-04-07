using System.Diagnostics;
using System.Net.Mime;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using TanksServer.Helpers;
using TanksServer.Models;

namespace TanksServer.Services;

internal sealed class PixelDrawer : ITickStep
{
    private const uint GameFieldPixelCount = MapService.PixelsPerRow * MapService.PixelsPerColumn;
    private DisplayPixelBuffer? _lastFrame;
    private readonly MapService _map;
    private readonly TankManager _tanks;
    private readonly bool[] _tankSprite;
    private readonly int _tankSpriteWidth;

    public PixelDrawer(MapService map, TankManager tanks, ILogger<PixelDrawer> logger)
    {
        _map = map;
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

    public DisplayPixelBuffer LastFrame
    {
        get => _lastFrame ?? throw new InvalidOperationException("first frame not yet drawn");
        private set => _lastFrame = value;
    }

    public Task TickAsync()
    {
        var buffer = CreateGameFieldPixelBuffer();
        DrawMap(buffer);
        DrawTanks(buffer);
        LastFrame = buffer;
        return Task.CompletedTask;
    }

    private void DrawMap(DisplayPixelBuffer buf)
    {
        for (var tileY = 0; tileY < MapService.TilesPerColumn; tileY++)
        for (var tileX = 0; tileX < MapService.TilesPerRow; tileX++)
        {
            var tile = new TilePosition(tileX, tileY);
            if (!_map.IsCurrentlyWall(tile))
                continue;

            var absoluteTilePixelY = tileY * MapService.TileSize;
            for (var pixelInTileY = 0; pixelInTileY < MapService.TileSize; pixelInTileY++)
            {
                var absoluteRowStartPixelIndex = (absoluteTilePixelY + pixelInTileY) * MapService.PixelsPerRow
                                                 + tileX * MapService.TileSize;
                for (var pixelInTileX = 0; pixelInTileX < MapService.TileSize; pixelInTileX++)
                    buf.Pixels[absoluteRowStartPixelIndex + pixelInTileX] = pixelInTileX % 2 == pixelInTileY % 2;
            }
        }
    }

    private void DrawTanks(DisplayPixelBuffer buf)
    {
        foreach (var tank in _tanks)
        {
            var pos = tank.Position.ToPixelPosition();
            var rotationVariant = (int)Math.Floor(tank.Rotation);
            for (var dy = 0; dy < MapService.TileSize; dy++)
            {
                var rowStartIndex = (pos.Y + dy) * MapService.PixelsPerRow;

                for (var dx = 0; dx < MapService.TileSize; dx++)
                {
                    if (!TankSpriteAt(dx, dy, rotationVariant))
                        continue;
                    
                    var i = rowStartIndex + pos.X + dx;
                    buf.Pixels[i] = true;
                }
            }
        }
    }

    private bool TankSpriteAt(int dx, int dy, int tankRotation)
    {
        var x = tankRotation % 4 * (MapService.TileSize + 1);
        var y = (int)Math.Floor(tankRotation / 4d) * (MapService.TileSize + 1);
        var index = (y + dy) * _tankSpriteWidth + x + dx;

        if (index < 0 || index > _tankSprite.Length)
            Debugger.Break();

        return _tankSprite[index];
    }

    private static DisplayPixelBuffer CreateGameFieldPixelBuffer()
    {
        var data = new byte[10 + GameFieldPixelCount / 8];
        var result = new DisplayPixelBuffer(data)
        {
            Magic1 = 0,
            Magic2 = 19,
            X = 0,
            Y = 0,
            WidthInTiles = MapService.TilesPerRow,
            HeightInPixels = MapService.PixelsPerColumn
        };
        return result;
    }
}
