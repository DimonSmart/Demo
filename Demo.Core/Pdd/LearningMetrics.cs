namespace Demo.Demos.Pdd;

/// <summary>
/// Contains metrics about the learning progress for questions
/// </summary>
public class LearningMetrics
{
    // Totals
    public int TotalQuestions { get; set; }
    public int LearnedQuestions { get; set; }
    public int InProgressQuestions { get; set; }
    public int NotStartedQuestions { get; set; }

    // Current session
    public int CorrectAnswers { get; set; }
    public int IncorrectAnswers { get; set; }
    public int TotalAnswers => CorrectAnswers + IncorrectAnswers;
}
