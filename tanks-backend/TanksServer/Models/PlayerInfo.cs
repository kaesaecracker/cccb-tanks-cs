namespace TanksServer.Models;

internal sealed record class TankInfo(
    int Orientation,
    byte ExplosiveBullets,
    PixelPosition Position,
    bool Moving
);

internal sealed record class PlayerInfo(
    string Name,
    Scores Scores,
    PlayerControls Controls,
    TankInfo? Tank
);
