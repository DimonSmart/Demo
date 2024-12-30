using System.Text.Json.Serialization;

namespace Demo.Demos.Pdd
{
    public class LocalizedText
    {
        [JsonPropertyName("R")]
        public string Russian { get; set; } = string.Empty;
        [JsonPropertyName("S")]
        public string Spanish { get; set; } = string.Empty;
        [JsonPropertyName("E")]
        public string English { get; set; } = string.Empty;
    }
}