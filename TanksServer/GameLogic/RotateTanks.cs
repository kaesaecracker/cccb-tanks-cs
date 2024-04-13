namespace TanksServer.GameLogic;

internal sealed class RotateTanks(
    TankManager tanks,
    IOptions<TanksConfiguration> options,
    ILogger<RotateTanks> logger
) : ITickStep
{
    private readonly TanksConfiguration _config = options.Value;

    public Task TickAsync()
    {
        foreach (var tank in tanks)
        {
            var player = tank.Owner;

            switch (player.Controls)
            {
                case { TurnRight: true, TurnLeft: true }:
                case { TurnRight: false, TurnLeft: false }:
                    continue;
                case { TurnLeft: true }:
                    tank.Rotation -= _config.TurnSpeed;
                    break;
                case { TurnRight: true }:
                    tank.Rotation += _config.TurnSpeed;
                    break;
            }

            logger.LogTrace("rotated tank to {}", tank.Rotation);
        }

        return Task.CompletedTask;
    }
}
