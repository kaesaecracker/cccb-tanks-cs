namespace TanksServer.Graphics;

internal sealed class GamePixel
{
    public Player? BelongsTo { get; set; }

    public GamePixelEntityType? EntityType { get; set; }

    public void Clear()
    {
        BelongsTo = null;
        EntityType = null;
    }
}
