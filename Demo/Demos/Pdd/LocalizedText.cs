using System.Text.Json.Serialization;

namespace Demo.Demos.Pdd;

[JsonConverter(typeof(LocalizedTextJsonConverter))]
public sealed class LocalizedText
{
    public Dictionary<string, string> Values { get; init; } = [];

    public string Get(string language) => Values.TryGetValue(language, out var value) ? value : string.Empty;
    public bool IsEmpty => Values.Values.All(string.IsNullOrWhiteSpace);
}
