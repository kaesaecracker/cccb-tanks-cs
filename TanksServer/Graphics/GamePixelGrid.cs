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

        _pixels = new GamePixel[height, width];
        for (var row = 0; row < height; row++)
        for (var column = 0; column < width; column++)
            _pixels[row, column] = new GamePixel();
    }

    public GamePixel this[int x, int y]
    {
        get
        {
            Debug.Assert(y * Width + x < _pixels.Length);
            return _pixels[y, x];
        }
    }

    public void Clear()
    {
        foreach (var pixel in _pixels)
            pixel.Clear();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IEnumerator<GamePixel> GetEnumerator()
    {
        for (var row = 0; row < Height; row++)
        for (var column = 0; column < Width; column++)
            yield return _pixels[row, column];
    }
}
