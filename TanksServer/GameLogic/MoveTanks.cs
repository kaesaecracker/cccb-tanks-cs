namespace TanksServer.GameLogic;

internal sealed class MoveTanks(
    TankManager tanks,
    IOptions<TanksConfiguration> options,
    MapService map
) : ITickStep
{
    private readonly TanksConfiguration _config = options.Value;

    public Task TickAsync()
    {
        foreach (var tank in tanks)
            tank.Moved = TryMoveTank(tank);

        return Task.CompletedTask;
    }

    private bool TryMoveTank(Tank tank)
    {
        var player = tank.Owner;

        double speed;
        switch (player.Controls)
        {
            case { Forward: false, Backward: false }:
            case { Forward: true, Backward: true }:
                return false;
            case { Forward: true }:
                speed = +_config.MoveSpeed;
                break;
            case { Backward: true }:
                speed = -_config.MoveSpeed;
                break;
            default:
                return false;
        }

        var angle = tank.Rotation * 2d * Math.PI;
        var newX = tank.Position.X + Math.Sin(angle) * speed;
        var newY = tank.Position.Y - Math.Cos(angle) * speed;

        return TryMoveTankTo(tank, new FloatPosition(newX, newY))
               || TryMoveTankTo(tank, new FloatPosition(newX, tank.Position.Y))
               || TryMoveTankTo(tank, new FloatPosition(tank.Position.X, newY));
    }

    private bool TryMoveTankTo(Tank tank, FloatPosition newPosition)
    {
        var (topLeft, bottomRight) = TankManager.GetTankBounds(newPosition.ToPixelPosition());

        if (HitsWall(topLeft, bottomRight))
            return false;
        if (HitsTank(tank, newPosition))
            return false;

        tank.Position = newPosition;
        return true;
    }

    private bool HitsTank(Tank tank, FloatPosition newPosition) =>
        tanks
            .Where(otherTank => otherTank != tank)
            .Any(otherTank => newPosition.Distance(otherTank.Position) < MapService.TileSize);

    private bool HitsWall(PixelPosition topLeft, PixelPosition bottomRight)
    {
        TilePosition[] positions =
        [
            topLeft.ToTilePosition(),
            new PixelPosition(bottomRight.X, topLeft.Y).ToTilePosition(),
            new PixelPosition(topLeft.X, bottomRight.Y).ToTilePosition(),
            bottomRight.ToTilePosition(),
        ];
        return positions.Any(map.IsCurrentlyWall);
    }
}