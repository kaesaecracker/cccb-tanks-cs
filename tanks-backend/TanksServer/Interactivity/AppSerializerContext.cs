using System.Text.Json.Serialization;

namespace TanksServer.Interactivity;

[JsonSerializable(typeof(Player))]
[JsonSerializable(typeof(IEnumerable<Player>))]
[JsonSerializable(typeof(Guid))]
[JsonSerializable(typeof(NameId))]
[JsonSerializable(typeof(IEnumerable<string>))]
[JsonSerializable(typeof(PlayerInfo))]
internal sealed partial class AppSerializerContext : JsonSerializerContext;
