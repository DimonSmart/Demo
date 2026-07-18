using Demo.Core.Quiz;

namespace Demo.Demos.Pdd;

/// <summary>
/// Contains information about a difficult question based on available data
/// </summary>
public class DifficultQuestion
{
    /// <summary>
    /// Question ID
    /// </summary>
    public string QuestionId { get; set; } = string.Empty;

    /// <summary>
    /// Question
    /// </summary>
    public QuizQuestion Question { get; set; } = new();

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
    public string TopicId { get; set; } = string.Empty;

    /// <summary>
    /// Topic this question belongs to
    /// </summary>
    public QuizTopic Topic { get; set; } = new();

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
