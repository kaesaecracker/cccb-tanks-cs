namespace TanksServer.Models;

internal record struct TankInfo(
    int Orientation,
    string Magazine,
    PixelPosition Position,
    bool Moving
);

internal record struct PlayerInfo(
    string Name,
    Scores Scores,
    string Controls,
    TankInfo? Tank,
    int OpenConnections
);
