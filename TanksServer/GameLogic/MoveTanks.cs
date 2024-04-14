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

        var angle = tank.Orientation / 16d * 2d * Math.PI;
        var newX = tank.Position.X + Math.Sin(angle) * speed;
        var newY = tank.Position.Y - Math.Cos(angle) * speed;

        return TryMoveTankTo(tank, new FloatPosition(newX, newY))
               || TryMoveTankTo(tank, new FloatPosition(newX, tank.Position.Y))
               || TryMoveTankTo(tank, new FloatPosition(tank.Position.X, newY));
    }

    private bool TryMoveTankTo(Tank tank, FloatPosition newPosition)
    {
        if (HitsWall(newPosition))
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

    private bool HitsWall(FloatPosition newPosition)
    {
        var (topLeft, _) = Tank.GetBoundsForCenter(newPosition);

        for (short y = 0; y < MapService.TileSize; y++)
        for (short x = 0; x < MapService.TileSize; x++)
        {
            var pixelToCheck = topLeft.GetPixelRelative(x, y);
            if (map.Current.IsCurrentlyWall(pixelToCheck))
                return true;
        }

        return false;
    }
}
