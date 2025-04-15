namespace Demo.Demos.Pdd;

/// <summary>
/// Contains metrics about the learning progress for questions
/// </summary>
public class LearningMetrics
{
    // Totals
    public int TotalQuestions { get; internal set; }
    public int LearnedQuestions { get; internal set; }
    public int InProgressQuestions { get; internal set; }
    public int NotStartedQuestions { get; internal set; }

    // Current session
    public int CorrectAnswers { get; internal set; }
    public int IncorrectAnswers { get; internal set; }
    public int TotalAnswers => CorrectAnswers + IncorrectAnswers;
}