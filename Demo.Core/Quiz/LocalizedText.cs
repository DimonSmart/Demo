using System.Text.Json.Serialization;

namespace Demo.Core.Quiz;

[JsonConverter(typeof(LocalizedTextJsonConverter))]
public sealed class LocalizedText
{
    public IReadOnlyDictionary<string, string> Values { get; init; } = new Dictionary<string, string>();

    public string Get(string language) =>
        Values.TryGetValue(language, out var value)
            ? value
            : Values.FirstOrDefault(pair => string.Equals(pair.Key, language, StringComparison.OrdinalIgnoreCase)).Value ?? string.Empty;

    public bool IsEmpty => Values.Values.All(string.IsNullOrWhiteSpace);
}
