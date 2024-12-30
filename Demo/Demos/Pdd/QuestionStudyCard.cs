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
    }
}