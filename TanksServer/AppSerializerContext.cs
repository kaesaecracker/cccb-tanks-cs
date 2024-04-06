using System.Text.Json.Serialization;

namespace TanksServer;

[JsonSerializable(typeof(Player))]
internal partial class AppSerializerContext: JsonSerializerContext;
