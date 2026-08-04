namespace Demo.Demos.Quiz;

/// <summary>
/// Service for generating and analyzing quiz learning statistics.
/// </summary>
public interface IQuizStatisticsService
{
    /// <summary>
    /// Generates detailed learning statistics based on study cards
    /// </summary>
    /// <param name="studyCards">User study cards</param>
    /// <returns>Detailed statistics</returns>
    Task<DetailedLearningStatistics> GenerateDetailedStatisticsAsync(Demo.Core.Quiz.QuizDocument document, List<QuestionStudyCard> studyCards);
}
