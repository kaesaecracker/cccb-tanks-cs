using System.Text.Json.Serialization;

namespace TanksServer.Models;

internal sealed class Player : IEquatable<Player>
{
    public required string Name { get; init; }

    [JsonIgnore] public PlayerControls Controls { get; } = new();

    public Scores Scores { get; } = new();

    public DateTime LastInput { get; set; } = DateTime.Now;

    public override bool Equals(object? obj) => obj is Player p && Equals(p);

    public bool Equals(Player? other) => other?.Name == Name;

    public override int GetHashCode() => Name.GetHashCode();

    public static bool operator ==(Player? left, Player? right) => Equals(left, right);

    public static bool operator !=(Player? left, Player? right) => !Equals(left, right);
}
