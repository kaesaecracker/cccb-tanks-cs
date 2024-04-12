namespace TanksServer.GameLogic;

internal sealed class RotateTanks(TankManager tanks, IOptions<TanksConfiguration> options) : ITickStep
{
    private readonly TanksConfiguration _config = options.Value;

    public Task TickAsync()
    {
        foreach (var tank in tanks)
        {
            var player = tank.Owner;

            if (player.Controls.TurnLeft)
                tank.Rotation -= _config.TurnSpeed / 16d;
            if (player.Controls.TurnRight)
                tank.Rotation += _config.TurnSpeed / 16d;
        }

        return Task.CompletedTask;
    }
}
