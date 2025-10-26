using System.Text.Json.Serialization;

namespace Buttercup.Email.Mailpit;

[JsonSerializable(typeof(SendRequestBody))]
[JsonSerializable(typeof(SendResponseBody))]
internal sealed partial class SerializerContext : JsonSerializerContext
{
}
