namespace TanksServer.GameLogic;

public interface ITickStep
{
    ValueTask TickAsync(TimeSpan delta);
}
