namespace TanksServer.GameLogic;

internal sealed class MapEntityManager(
    ILogger<MapEntityManager> logger,
    MapService map,
    IOptions<GameRules> options
)
{
    private readonly GameRules _rules = options.Value;
    private readonly HashSet<Bullet> _bullets = [];
    private readonly HashSet<PowerUp> _powerUps = [];
    private readonly Dictionary<Player, Tank> _playerTanks = [];
    private readonly TimeSpan _bulletTimeout = TimeSpan.FromMilliseconds(options.Value.BulletTimeoutMs);

    public IEnumerable<Bullet> Bullets => _bullets;
    public IEnumerable<Tank> Tanks => _playerTanks.Values;
    public IEnumerable<PowerUp> PowerUps => _powerUps;

    public void SpawnBullet(Player tankOwner, FloatPosition position, double rotation, MagazineType type)
    {
        var speed = _rules.BulletSpeed * (type.HasFlag(MagazineType.Fast) ? 2 : 1);
        _bullets.Add(new Bullet
        {
            Owner = tankOwner,
            Position = position,
            Rotation = rotation,
            IsExplosive = type.HasFlag(MagazineType.Explosive),
            Timeout = DateTime.Now + _bulletTimeout,
            OwnerCollisionAfter = DateTime.Now + TimeSpan.FromSeconds(1),
            Speed = speed
        });
    }

    public void RemoveWhere(Predicate<Bullet> predicate) => _bullets.RemoveWhere(predicate);

    public void SpawnTank(Player player)
    {
        var tank = new Tank
        {
            Owner = player,
            Position = ChooseSpawnPosition(),
            Rotation = Random.Shared.NextDouble(),
            Magazine = new Magazine(MagazineType.Basic, 0, _rules.MagazineSize)
        };
        _playerTanks[player] = tank;
        logger.LogInformation("Tank added for player {}", player.Name);
    }

    public void SpawnPowerUp(PowerUpType type, MagazineType? magazineType) => _powerUps.Add(new PowerUp
    {
        Position = ChooseSpawnPosition(),
        Type = type,
        MagazineType = magazineType
    });

    public void RemoveWhere(Predicate<PowerUp> predicate) => _powerUps.RemoveWhere(predicate);

    public void Remove(Tank tank)
    {
        logger.LogInformation("Tank removed for player {}", tank.Owner.Name);
        _playerTanks.Remove(tank.Owner);
    }

    public Tank? GetCurrentTankOfPlayer(Player player) => _playerTanks.GetValueOrDefault(player);

    private IEnumerable<IMapEntity> AllEntities => Bullets
        .Cast<IMapEntity>()
        .Concat(Tanks)
        .Concat(PowerUps);

    private FloatPosition ChooseSpawnPosition()
    {
        var maxMinDistance = 0d;
        TilePosition spawnTile = default;
        for (ushort x = 1; x < MapService.TilesPerRow - 1; x++)
        for (ushort y = 1; y < MapService.TilesPerColumn - 1; y++)
        {
            var tile = new TilePosition(x, y);
            if (map.Current.IsWall(tile))
                continue;

            var tilePixelCenter = tile.ToPixelPosition().GetPixelRelative(4, 4).ToFloatPosition();
            var minDistance = AllEntities
                .Select(entity => entity.Position.Distance(tilePixelCenter))
                .Aggregate(double.MaxValue, Math.Min);
            if (minDistance <= maxMinDistance)
                continue;

            maxMinDistance = minDistance;
            spawnTile = tile;
        }

        return spawnTile.ToPixelPosition().GetPixelRelative(4, 4).ToFloatPosition();
    }
}
