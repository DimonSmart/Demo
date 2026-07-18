using System.Text.Json;
using System.Text.Json.Serialization;

namespace Demo.Core.Quiz;

public sealed class LocalizedTextJsonConverter : JsonConverter<LocalizedText>
{
    public override LocalizedText Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var values = JsonSerializer.Deserialize<Dictionary<string, string>>(ref reader, options)
            ?? new Dictionary<string, string>();
        return new LocalizedText { Values = values };
    }

    public override void Write(Utf8JsonWriter writer, LocalizedText value, JsonSerializerOptions options) =>
        JsonSerializer.Serialize(writer, value.Values, options);
}
