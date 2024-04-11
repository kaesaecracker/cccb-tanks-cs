namespace TanksServer.Models;

internal sealed class Bullet(Player tankOwner, FloatPosition position, double rotation): IMapEntity
{
    public Player Owner { get; } = tankOwner;
    
    public FloatPosition Position { get; set; } = position;
    
    public double Rotation { get; set; } = rotation;
}