using System.Text.Json.Serialization;

namespace TanksServer.Models;

internal sealed class Player(string name, Guid id)
{
    public string Name => name;

    [JsonIgnore] public Guid Id => id;

    [JsonIgnore] public PlayerControls Controls { get; } = new();

    public Scores Scores { get; } = new();

    public DateTime LastInput { get; set; } = DateTime.Now;
}
