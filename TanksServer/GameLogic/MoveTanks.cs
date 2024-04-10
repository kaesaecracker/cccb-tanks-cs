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

        var angle = tank.Rotation / 16d * 2d * Math.PI;
        var newX = tank.Position.X + Math.Sin(angle) * speed;
        var newY = tank.Position.Y - Math.Cos(angle) * speed;

        return TryMoveTankTo(tank, new FloatPosition(newX, newY))
               || TryMoveTankTo(tank, tank.Position with { X = newX })
               || TryMoveTankTo(tank, tank.Position with { Y = newY });
    }

    private bool TryMoveTankTo(Tank tank, FloatPosition newPosition)
    {
        var x0 = (int)Math.Floor(newPosition.X / MapService.TileSize);
        var x1 = (int)Math.Ceiling(newPosition.X / MapService.TileSize);
        var y0 = (int)Math.Floor(newPosition.Y / MapService.TileSize);
        var y1 = (int)Math.Ceiling(newPosition.Y / MapService.TileSize);

        TilePosition[] positions = { new(x0, y0), new(x0, y1), new(x1, y0), new(x1, y1) };
        if (positions.Any(map.IsCurrentlyWall))
            return false;

        tank.Position = newPosition;
        return true;
    }
}
