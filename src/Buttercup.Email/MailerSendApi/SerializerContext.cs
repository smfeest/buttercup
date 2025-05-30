using System.Text.Json.Serialization;

namespace Buttercup.Email.MailerSendApi;

[JsonSerializable(typeof(EmailRequestBody))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal sealed partial class SerializerContext : JsonSerializerContext
{
}
