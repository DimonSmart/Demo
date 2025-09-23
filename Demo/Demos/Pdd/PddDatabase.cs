using System.Text.Json.Serialization;

namespace Demo.Demos.Pdd;

public class PddDatabase
{
    [JsonPropertyName("Topics")]
    public List<Topic> Topics { get; set; } = new List<Topic>();

    [JsonPropertyName("Questions")]
    public List<QuestionItem> Questions { get; set; } = new List<QuestionItem>();
}