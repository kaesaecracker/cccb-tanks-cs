using System.Text.Json.Serialization;

namespace TanksServer.Interactivity;

[JsonSerializable(typeof(Player))]
[JsonSerializable(typeof(IEnumerable<Player>))]
[JsonSerializable(typeof(Guid))]
internal sealed partial class AppSerializerContext : JsonSerializerContext;
