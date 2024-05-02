using System.Text.Json.Serialization;

namespace TanksServer.Models;

internal sealed class Player : IEquatable<Player>
{
    private int _openConnections;

    public required string Name { get; init; }

    [JsonIgnore] public PlayerControls Controls { get; } = new();

    public Scores Scores { get; } = new();

    public DateTime LastInput { get; set; } = DateTime.Now;

    public int OpenConnections => _openConnections;

    public override bool Equals(object? obj) => obj is Player p && Equals(p);

    public bool Equals(Player? other) => other?.Name == Name;

    public override int GetHashCode() => Name.GetHashCode();

    public static bool operator ==(Player? left, Player? right) => Equals(left, right);

    public static bool operator !=(Player? left, Player? right) => !Equals(left, right);

    internal void IncrementConnectionCount() => Interlocked.Increment(ref _openConnections);

    internal void DecrementConnectionCount() => Interlocked.Decrement(ref _openConnections);
}
