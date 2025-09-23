namespace Demo.Demos.Pdd;

/// <summary>
/// Service for generating and analyzing PDD learning statistics
/// </summary>
public interface IPddStatisticsService
{
    /// <summary>
    /// Generates detailed learning statistics based on study cards
    /// </summary>
    /// <param name="studyCards">User study cards</param>
    /// <returns>Detailed statistics</returns>
    Task<DetailedLearningStatistics> GenerateDetailedStatisticsAsync(List<QuestionStudyCard> studyCards);
}