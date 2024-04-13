using System.Text.Json.Serialization;

namespace TanksServer.Interactivity;

[JsonSerializable(typeof(Player))]
internal sealed partial class AppSerializerContext : JsonSerializerContext;
