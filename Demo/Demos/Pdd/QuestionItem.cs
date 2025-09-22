using System.Text.Json.Serialization;

namespace Demo.Demos.Pdd
{
    public class QuestionItem
    {
        public int Id { get; set; }
        
        [JsonPropertyName("Q")]
        public LocalizedText LocalizedQuestionText { get; set; } = new LocalizedText();
        
        public string InitialQuestionText { get; set; } = string.Empty;
        public string? QuestionCheckDescription { get; set; }
        public bool Img { get; set; }
        
        [JsonPropertyName("Rule")]
        public LocalizedText RuleDescription { get; set; } = new LocalizedText();
        
        [JsonPropertyName("A")]
        public List<AnswerItem> Answers { get; set; } = new List<AnswerItem>();
        
        [JsonPropertyName("Terms")]
        public List<LocalizedText> MainTerms { get; set; } = [];
        
        /// <summary>
        /// ID of the topic this question belongs to
        /// </summary>
        public int TopicId { get; set; }
    }
}