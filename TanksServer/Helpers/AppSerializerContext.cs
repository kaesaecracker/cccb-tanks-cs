using System.Text.Json.Serialization;
using TanksServer.Models;

namespace TanksServer.Helpers;

[JsonSerializable(typeof(Player))]
internal sealed partial class AppSerializerContext: JsonSerializerContext;
