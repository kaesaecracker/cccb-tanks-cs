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

internal enum GamePixelEntityType : byte
{
    Wall = 0x0,
    Tank = 0x1,
    Bullet = 0x2
}
