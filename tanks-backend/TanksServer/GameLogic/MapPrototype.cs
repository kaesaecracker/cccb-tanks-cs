namespace TanksServer.GameLogic;

internal abstract class MapPrototype
{
    public abstract string Name { get; }

    public abstract Map CreateInstance();
}
