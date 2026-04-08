namespace Demo.Demos.Pdd
{

    /// <summary>
    /// Represents a study card for a question (learning progress).
    /// </summary>
    public class QuestionStudyCard
    {
        public int Id { get; set; }
        public bool IsLearned => ConsecutiveCorrectCount >= 3;
        public int ConsecutiveCorrectCount { get; set; }
        
        /// <summary>
        /// Date and time of last answer to the question.
        /// May be null for existing users.
        /// </summary>
        public DateTime? LastAnsweredAt { get; set; }
    }
}