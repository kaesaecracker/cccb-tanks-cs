namespace TanksServer.GameLogic;

internal sealed class ChangeToRequestedMap(
    MapService mapService,
    MapEntityManager entityManager,
    EmptyTileFinder emptyTileFinder
) : ITickStep
{
    private MapPrototype? _requestedMap;

    public ValueTask TickAsync(TimeSpan delta)
    {
        var changeTo = Interlocked.Exchange(ref _requestedMap, null);
        if (changeTo == null)
            return ValueTask.CompletedTask;

        mapService.SwitchTo(changeTo);
        foreach (var t in entityManager.Tanks)
            t.Position = emptyTileFinder.ChooseEmptyTile().GetCenter().ToFloatPosition();
        return ValueTask.CompletedTask;
    }

    public void Request(MapPrototype map) => _requestedMap = map;
}
