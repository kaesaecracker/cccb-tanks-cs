using System.Text.Json.Serialization;

namespace TanksServer.Models;

internal sealed class Player(string name)
{
    public string Name => name;

    public Guid Id { get; } = Guid.NewGuid();

    [JsonIgnore] public PlayerControls Controls { get; } = new();

    public int Kills { get; set; }

    public int Deaths { get; set; }

    public DateTime LastInput { get; set; } = DateTime.Now;
}
