namespace TanksServer.GameLogic;

public interface ITickStep
{
    Task TickAsync(TimeSpan delta);
}
