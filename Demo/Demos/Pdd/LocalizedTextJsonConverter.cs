using System.Text.Json;
using System.Text.Json.Serialization;

namespace Demo.Demos.Pdd;

public sealed class LocalizedTextJsonConverter : JsonConverter<LocalizedText>
{
    public override LocalizedText Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var values = JsonSerializer.Deserialize<Dictionary<string, string>>(ref reader, options)
            ?? throw new JsonException("Localized text must be an object.");
        return new LocalizedText { Values = values };
    }

    public override void Write(Utf8JsonWriter writer, LocalizedText value, JsonSerializerOptions options) =>
        JsonSerializer.Serialize(writer, value.Values, options);
}
