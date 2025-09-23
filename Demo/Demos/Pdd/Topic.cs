using System.Text.Json.Serialization;

namespace Demo.Demos.Pdd;

public class Topic
{
    public int Id { get; set; }
    public string Slug { get; set; } = string.Empty;

    [JsonPropertyName("Title")]
    public LocalizedText Title { get; set; } = new LocalizedText();
}