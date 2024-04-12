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

        for (ushort x = 0; x < MapService.TilesPerRow; x++)
        for (ushort y = 0; y < MapService.TilesPerColumn; y++)
        {
            var tile = new TilePosition(x, y);

            if (map.IsCurrentlyWall(tile))
                continue;

            var tilePixelCenter = tile.GetPixelRelative(4, 4);

            var minDistance = bullets.GetAll()
                .Cast<IMapEntity>()
                .Concat(tanks)
                .Select(entity => Math.Sqrt(
                    Math.Pow(entity.Position.X - tilePixelCenter.X, 2) +
                    Math.Pow(entity.Position.Y - tilePixelCenter.Y, 2)))
                .Aggregate(double.MaxValue, Math.Min);
            
            candidates.Add(tile, minDistance);
        }

        var min = candidates.MaxBy(kvp => kvp.Value).Key;
        return new FloatPosition(
            min.X * MapService.TileSize,
            min.Y * MapService.TileSize
        );
    }
}