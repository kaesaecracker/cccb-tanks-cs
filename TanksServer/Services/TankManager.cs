using System.Collections;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TanksServer.Models;

namespace TanksServer.Services;

internal sealed class TankManager(
    ILogger<TankManager> logger,
    IOptions<TanksConfiguration> options,
    MapService map,
    BulletManager bullets
) : ITickStep, IEnumerable<Tank>
{
    private readonly ConcurrentBag<Tank> _tanks = new();
    private readonly TanksConfiguration _config = options.Value;

    public void Add(Tank tank)
    {
        logger.LogInformation("Tank added for player {}", tank.Owner.Id);
        _tanks.Add(tank);
    }

    public Task TickAsync()
    {
        foreach (var tank in _tanks)
        {
            if (TryMoveTank(tank))
                continue;
            Shoot(tank);
        }

        return Task.CompletedTask;
    }

    private bool TryMoveTank(Tank tank)
    {
        logger.LogTrace("moving tank for player {}", tank.Owner.Id);
        var player = tank.Owner;

        if (player.Controls.TurnLeft)
            tank.Rotation -= _config.TurnSpeed;
        if (player.Controls.TurnRight)
            tank.Rotation += _config.TurnSpeed;

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

        return TryMove(tank, new FloatPosition(newX, newY))
               || TryMove(tank, tank.Position with { X = newX })
               || TryMove(tank, tank.Position with { Y = newY });
    }

    private bool TryMove(Tank tank, FloatPosition newPosition)
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

    private void Shoot(Tank tank)
    {
        if (!tank.Owner.Controls.Shoot)
            return;
        if (tank.NextShotAfter >= DateTime.Now)
            return;

        tank.NextShotAfter = DateTime.Now.AddMilliseconds(_config.ShootDelayMs);

        var angle = tank.Rotation / 16 * 2 * Math.PI;
        var position = new FloatPosition(
            X: tank.Position.X + MapService.TileSize / 2d + Math.Sin(angle) * _config.BulletSpeed,
            Y: tank.Position.Y + MapService.TileSize / 2d - Math.Cos(angle) * _config.BulletSpeed
        );

        bullets.Spawn(new Bullet(tank.Owner, position, tank.Rotation));
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public IEnumerator<Tank> GetEnumerator() => _tanks.GetEnumerator();
}
