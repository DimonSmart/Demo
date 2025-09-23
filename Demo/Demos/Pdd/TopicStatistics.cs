namespace Demo.Demos.Pdd;

/// <summary>
/// Contains study statistics for a specific topic based on available data
/// </summary>
public class TopicStatistics
{
    /// <summary>
    /// Topic information
    /// </summary>
    public Topic Topic { get; set; } = new();

    /// <summary>
    /// Total number of questions in the topic
    /// </summary>
    public int TotalQuestions { get; set; }

    /// <summary>
    /// Number of learned questions
    /// </summary>
    public int LearnedQuestions { get; set; }

    /// <summary>
    /// Number of questions in progress (has progress but not yet learned)
    /// </summary>
    public int InProgressQuestions { get; set; }

    /// <summary>
    /// Number of questions not yet started studying
    /// </summary>
    public int NotStartedQuestions { get; set; }

    /// <summary>
    /// Topic learning percentage (0-100%)
    /// </summary>
    public double LearnedPercentage => TotalQuestions > 0 ? (double)LearnedQuestions / TotalQuestions * 100 : 0;

    /// <summary>
    /// Average learning progress in the topic (average ConsecutiveCorrectCount value)
    /// </summary>
    public double AverageLearningProgress { get; set; }

    /// <summary>
    /// Number of questions studied recently (within a week)
    /// </summary>
    public int RecentlyStudiedQuestions { get; set; }
}