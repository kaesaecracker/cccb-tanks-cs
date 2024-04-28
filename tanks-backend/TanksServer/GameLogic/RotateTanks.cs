namespace TanksServer.GameLogic;

internal sealed class RotateTanks(
    MapEntityManager entityManager,
    IOptions<GameRules> options,
    ILogger<RotateTanks> logger
) : ITickStep
{
    private readonly GameRules _config = options.Value;

    public ValueTask TickAsync(TimeSpan delta)
    {
        foreach (var tank in entityManager.Tanks)
        {
            var player = tank.Owner;

            switch (player.Controls)
            {
                case { TurnRight: true, TurnLeft: true }:
                case { TurnRight: false, TurnLeft: false }:
                    continue;
                case { TurnLeft: true }:
                    tank.Rotation -= _config.TurnSpeed * delta.TotalSeconds;
                    break;
                case { TurnRight: true }:
                    tank.Rotation += _config.TurnSpeed * delta.TotalSeconds;
                    break;
            }

            logger.LogTrace("rotated tank to {}", tank.Rotation);
        }

        return ValueTask.CompletedTask;
    }
}
