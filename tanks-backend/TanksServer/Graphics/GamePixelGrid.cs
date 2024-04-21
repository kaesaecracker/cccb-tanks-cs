using System.Collections;
using System.Diagnostics;

namespace TanksServer.Graphics;

internal sealed class GamePixelGrid : IEnumerable<GamePixel>
{
    public int Width { get; }
    public int Height { get; }

    private readonly GamePixel[,] _pixels;

    public GamePixelGrid(int width, int height)
    {
        Width = width;
        Height = height;

        _pixels = new GamePixel[width, height];
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
            this[x, y] = new GamePixel();
    }

    public GamePixel this[int x, int y]
    {
        get
        {
            Debug.Assert(y * Width + x < _pixels.Length);
            return _pixels[x, y];
        }
        set => _pixels[x, y] = value;
    }

    public void Clear()
    {
        foreach (var pixel in _pixels)
            pixel.Clear();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IEnumerator<GamePixel> GetEnumerator()
    {
        for (var y = 0; y < Height; y++)
        for (var x = 0; x < Width; x++)
            yield return this[x, y];
    }
}
