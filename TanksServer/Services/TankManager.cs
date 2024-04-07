using System.Collections;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TanksServer.Models;

namespace TanksServer.Services;

internal sealed class TankManager(ILogger<TankManager> logger, IOptions<TanksConfiguration> options)
    : ITickStep, IEnumerable<Tank>
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
            TryMoveTank(tank);
        }

        return Task.CompletedTask;
    }

    private bool TryMoveTank(Tank tank)
    {
        logger.LogTrace("moving tank for player {}", tank.Owner.Id);
        var player = tank.Owner;

        // move turret
        if (player.Controls.TurnLeft) Rotate(tank, -_config.TurnSpeed);
        if (player.Controls.TurnRight) Rotate(tank, +_config.TurnSpeed);

        if (player.Controls is { Forward: false, Backward: false })
            return false;

        var direction = player.Controls.Forward ? 1 : -1;
        var angle = tank.Rotation / 16d * 2d * Math.PI;
        var newX = tank.Position.X + Math.Sin(angle) * direction * _config.MoveSpeed;
        var newY = tank.Position.Y - Math.Cos(angle) * direction * _config.MoveSpeed;

        return TryMove(tank, newX, newY)
               || TryMove(tank, newX, tank.Position.Y)
               || TryMove(tank, tank.Position.X, newY);
    }

    private static bool TryMove(Tank tank, double newX, double newY)
    {
        // TODO implement

        tank.Position = new FloatPosition(newX, newY);
        return true;
    }

    private void Rotate(Tank t, double speed)
    {
        var newRotation = (t.Rotation + speed + 16) % 16;
        logger.LogTrace("rotating tank for {} from {} to {}", t.Owner.Id, t.Rotation, newRotation);
        t.Rotation = newRotation;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public IEnumerator<Tank> GetEnumerator() => _tanks.GetEnumerator();
}
