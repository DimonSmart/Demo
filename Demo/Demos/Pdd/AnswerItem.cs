using System.Text.Json.Serialization;

namespace Demo.Demos.Pdd
{
    public class AnswerItem
    {
        [JsonPropertyName("Y")]
        public bool IsCorrect { get; set; }
        [JsonPropertyName("T")]
        public LocalizedText LocalizedAnswerText { get; set; } = new LocalizedText();
    }
}