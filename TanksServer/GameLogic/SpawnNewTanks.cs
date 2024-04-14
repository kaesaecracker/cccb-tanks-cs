namespace TanksServer.GameLogic;

internal sealed class SpawnNewTanks(
    TankManager tanks,
    MapService map,
    SpawnQueue queue,
    BulletManager bullets
) : ITickStep
{
    public Task TickAsync()
    {
        if (!queue.TryDequeueNext(out var player))
            return Task.CompletedTask;

        tanks.Add(new Tank(player, ChooseSpawnPosition())
        {
            Rotation = Random.Shared.NextDouble()
        });

        return Task.CompletedTask;
    }

    private FloatPosition ChooseSpawnPosition()
    {
        Dictionary<TilePosition, double> candidates = [];

        for (ushort x = 1; x < MapService.TilesPerRow - 1; x++)
        for (ushort y = 1; y < MapService.TilesPerColumn - 1; y++)
        {
            var tile = new TilePosition(x, y);
            if (map.Current.IsWall(tile))
                continue;

            var tilePixelCenter = tile.ToPixelPosition().GetPixelRelative(4, 4).ToFloatPosition();

            var minDistance = bullets.GetAll()
                .Cast<IMapEntity>()
                .Concat(tanks)
                .Select(entity => entity.Position.Distance(tilePixelCenter))
                .Aggregate(double.MaxValue, Math.Min);

            candidates.Add(tile, minDistance);
        }

        var min = candidates.MaxBy(kvp => kvp.Value).Key;
        return min.ToPixelPosition().GetPixelRelative(4, 4).ToFloatPosition();
    }
}
