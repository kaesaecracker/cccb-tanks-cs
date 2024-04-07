using System.Text.Json.Serialization;

namespace TanksServer.Helpers;

[JsonSerializable(typeof(Player))]
internal sealed partial class AppSerializerContext: JsonSerializerContext;
