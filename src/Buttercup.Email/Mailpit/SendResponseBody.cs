using System.Text.Json.Serialization;

namespace Buttercup.Email.Mailpit;

internal sealed record SendResponseBody([property: JsonPropertyName("ID")] string Id);
