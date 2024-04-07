namespace TanksServer.TickSteps;

internal sealed class RotateTanks(TankManager tanks, IOptions<TanksConfiguration> options) : ITickStep
{
    private readonly TanksConfiguration _config = options.Value;

    public Task TickAsync()
    {
        foreach (var tank in tanks)
        {
            var player = tank.Owner;

            if (player.Controls.TurnLeft)
                tank.Rotation -= _config.TurnSpeed;
            if (player.Controls.TurnRight)
                tank.Rotation += _config.TurnSpeed;
        }

        return Task.CompletedTask;
    }
}
