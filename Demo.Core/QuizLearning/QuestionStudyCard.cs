namespace Demo.Demos.Quiz
{

    /// <summary>
    /// Represents a study card for a question (learning progress).
    /// </summary>
    public class QuestionStudyCard
    {
        public string Id { get; set; } = string.Empty;
        public bool IsLearned => ConsecutiveCorrectCount >= 3;
        public int ConsecutiveCorrectCount { get; set; }
        
        /// <summary>
        /// Date and time of last answer to the question.
        /// May be null for existing users.
        /// </summary>
        public DateTime? LastAnsweredAt { get; set; }
    }
}
