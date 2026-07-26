using System.Text.Json;
using System.Text.Json.Serialization;

namespace Buttercup.EntityModel;

public sealed class ChangedValueJsonConverter<T> : JsonConverter<ChangedValue<T>>
{
    public override ChangedValue<T>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.StartArray)
        {
            reader.Read();
            var previousValue = JsonSerializer.Deserialize<T>(ref reader, options);
            reader.Read();
            var newValue = JsonSerializer.Deserialize<T>(ref reader, options);
            reader.Read();
            return new ChangedValue<T>(previousValue, newValue);
        }
        else
        {
            // TODO: Consider one element array for new values too, so that array-typed values can be safely represented in future
            var newValue = JsonSerializer.Deserialize<T>(ref reader, options);
            return new ChangedValue<T>(newValue);
        }
    }

    public override void Write(Utf8JsonWriter writer, ChangedValue<T> value, JsonSerializerOptions options)
    {
        if (value.HasPreviousValue)
        {
            writer.WriteStartArray();
            JsonSerializer.Serialize(writer, value.PreviousValue);
            JsonSerializer.Serialize(writer, value.NewValue);
            writer.WriteEndArray();
        }
        else
        {
            // TODO: Consider one element array for new values too, so that array-typed values can be safely represented in future
            JsonSerializer.Serialize(writer, value.NewValue);
        }
    }
}
