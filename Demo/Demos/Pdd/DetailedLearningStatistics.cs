namespace Demo.Demos.Pdd;

/// <summary>
/// Extended learning statistics including breakdown by topics and difficult questions
/// </summary>
public class DetailedLearningStatistics
{
    /// <summary>
    /// Overall statistics (existing)
    /// </summary>
    public LearningMetrics OverallMetrics { get; set; } = new();

    /// <summary>
    /// Statistics for each topic
    /// </summary>
    public List<TopicStatistics> TopicStatistics { get; set; } = new();

    /// <summary>
    /// Top difficult questions (with most errors and not yet learned)
    /// </summary>
    public List<DifficultQuestion> MostDifficultQuestions { get; set; } = new();

    /// <summary>
    /// Statistics creation date
    /// </summary>
    public DateTime GeneratedAt { get; set; } = DateTime.Now;
}