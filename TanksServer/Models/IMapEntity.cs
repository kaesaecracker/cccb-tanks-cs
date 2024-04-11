namespace TanksServer.Models;

internal interface IMapEntity
{
    FloatPosition Position { get; set; }
    double Rotation { get; set; }
}