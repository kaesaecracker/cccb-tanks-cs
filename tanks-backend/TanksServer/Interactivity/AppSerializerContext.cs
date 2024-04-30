using System.Text.Json;
using System.Text.Json.Serialization;

namespace TanksServer.Interactivity;

[JsonSerializable(typeof(Player))]
[JsonSerializable(typeof(IEnumerable<Player>))]
[JsonSerializable(typeof(IEnumerable<string>))]
[JsonSerializable(typeof(PlayerInfo))]
[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
internal sealed partial class AppSerializerContext : JsonSerializerContext;
