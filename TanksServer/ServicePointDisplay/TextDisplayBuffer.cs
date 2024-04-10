namespace TanksServer.ServicePointDisplay;

internal sealed class TextDisplayBuffer : DisplayBufferView
{
    public TextDisplayBuffer(TilePosition position, ushort charsPerRow, ushort rows)
        : base(new byte[10 + charsPerRow * rows])
    {
        Mode = 3;
        WidthInTiles = charsPerRow;
        RowCount = rows;
        Position = position;
        Rows = new FixedSizeCharGridView(Data.AsMemory(10), charsPerRow, rows);
    }

    public FixedSizeCharGridView Rows { get; set; }
}