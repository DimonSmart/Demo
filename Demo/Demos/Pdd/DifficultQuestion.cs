namespace Demo.Demos.Pdd;

/// <summary>
/// Contains information about a difficult question based on available data
/// </summary>
public class DifficultQuestion
{
    /// <summary>
    /// Question ID
    /// </summary>
    public int QuestionId { get; set; }

    /// <summary>
    /// Question
    /// </summary>
    public QuestionItem Question { get; set; } = new();

    /// <summary>
    /// Number of study attempts (based on ConsecutiveCorrectCount and learning status)
    /// </summary>
    public int EstimatedAttempts { get; set; }

    /// <summary>
    /// Difficulty based on low learning progress.
    /// High value = more attempts needed for learning or long time without study
    /// </summary>
    public int DifficultyScore { get; set; }

    /// <summary>
    /// Whether the question is learned
    /// </summary>
    public bool IsLearned { get; set; }

    /// <summary>
    /// ID of the topic this question belongs to
    /// </summary>
    public int TopicId { get; set; }

    /// <summary>
    /// Topic this question belongs to
    /// </summary>
    public Topic Topic { get; set; } = new();

    /// <summary>
    /// Date of last answer (can be null)
    /// </summary>
    public DateTime? LastAnsweredAt { get; set; }

    /// <summary>
    /// How many days ago the last answer was given (if known)
    /// </summary>
    public int? DaysSinceLastAnswer => LastAnsweredAt.HasValue 
        ? (int)(DateTime.Now - LastAnsweredAt.Value).TotalDays 
        : null;
}