namespace Demo.Demos.Pdd;

public class PddStatisticsService : IPddStatisticsService
{
    private readonly IPddDataService _pddDataService;

    public PddStatisticsService(IPddDataService pddDataService)
    {
        _pddDataService = pddDataService;
    }

    public async Task<DetailedLearningStatistics> GenerateDetailedStatisticsAsync(List<QuestionStudyCard> studyCards)
    {
        var database = await _pddDataService.LoadDatabaseAsync();
        
        var allQuestions = database.Questions.ToList();
        var allTopics = database.Topics.ToList();
        
        // Create dictionary of study cards for fast access
        var studyCardsByQuestionId = studyCards.ToDictionary(sc => sc.Id, sc => sc);

        // Generate overall metrics
        var overallMetrics = GenerateOverallMetrics(allQuestions, studyCardsByQuestionId);

        // Generate topic statistics
        var topicStats = GenerateTopicStatistics(allQuestions, allTopics, studyCardsByQuestionId);

        // Generate list of most difficult questions
        var difficultQuestions = GenerateDifficultQuestions(allQuestions, allTopics, studyCardsByQuestionId);

        return new DetailedLearningStatistics
        {
            OverallMetrics = overallMetrics,
            TopicStatistics = topicStats,
            MostDifficultQuestions = difficultQuestions
        };
    }

    private LearningMetrics GenerateOverallMetrics(List<QuestionItem> allQuestions, Dictionary<int, QuestionStudyCard> studyCards)
    {
        var totalQuestions = allQuestions.Count;
        var learnedQuestions = 0;
        var inProgressQuestions = 0;
        var notStartedQuestions = 0;

        foreach (var question in allQuestions)
        {
            if (studyCards.TryGetValue(question.Id, out var card))
            {
                if (card.IsLearned)
                    learnedQuestions++;
                else if (card.ConsecutiveCorrectCount > 0)
                    inProgressQuestions++;
                else
                    notStartedQuestions++;
            }
            else
            {
                notStartedQuestions++;
            }
        }

        return new LearningMetrics
        {
            TotalQuestions = totalQuestions,
            LearnedQuestions = learnedQuestions,
            InProgressQuestions = inProgressQuestions,
            NotStartedQuestions = notStartedQuestions,
            CorrectAnswers = 0, // This data is only available in current session through QuestionQueue
            IncorrectAnswers = 0
        };
    }

    private List<TopicStatistics> GenerateTopicStatistics(List<QuestionItem> allQuestions, 
        List<Topic> allTopics, Dictionary<int, QuestionStudyCard> studyCards)
    {
        var topicStats = new List<TopicStatistics>();
        var weekAgo = DateTime.Now.AddDays(-7);

        foreach (var topic in allTopics)
        {
            var topicQuestions = allQuestions.Where(q => q.TopicId == topic.Id).ToList();
            var totalQuestions = topicQuestions.Count;
            
            if (totalQuestions == 0) continue;

            var learnedQuestions = 0;
            var inProgressQuestions = 0;
            var notStartedQuestions = 0;
            var recentlyStudiedQuestions = 0;
            var totalProgress = 0.0;

            foreach (var question in topicQuestions)
            {
                if (studyCards.TryGetValue(question.Id, out var card))
                {
                    totalProgress += card.ConsecutiveCorrectCount;

                    if (card.IsLearned)
                        learnedQuestions++;
                    else if (card.ConsecutiveCorrectCount > 0)
                        inProgressQuestions++;
                    else
                        notStartedQuestions++;

                    // Check recent activity
                    if (card.LastAnsweredAt.HasValue && card.LastAnsweredAt.Value >= weekAgo)
                        recentlyStudiedQuestions++;
                }
                else
                {
                    notStartedQuestions++;
                }
            }

            var avgProgress = totalQuestions > 0 ? totalProgress / totalQuestions : 0;

            topicStats.Add(new TopicStatistics
            {
                Topic = topic,
                TotalQuestions = totalQuestions,
                LearnedQuestions = learnedQuestions,
                InProgressQuestions = inProgressQuestions,
                NotStartedQuestions = notStartedQuestions,
                AverageLearningProgress = avgProgress,
                RecentlyStudiedQuestions = recentlyStudiedQuestions
            });
        }

        return topicStats.OrderByDescending(ts => ts.LearnedPercentage).ToList();
    }

    private List<DifficultQuestion> GenerateDifficultQuestions(List<QuestionItem> allQuestions,
        List<Topic> allTopics, Dictionary<int, QuestionStudyCard> studyCards)
    {
        var difficultQuestions = new List<DifficultQuestion>();
        var topicDict = allTopics.ToDictionary(t => t.Id, t => t);

        foreach (var question in allQuestions)
        {
            if (!topicDict.TryGetValue(question.TopicId, out var topic))
                continue;

            if (studyCards.TryGetValue(question.Id, out var card))
            {
                // Skip already learned questions
                if (card.IsLearned) continue;

                // Skip questions that haven't been attempted yet (not truly "difficult")
                if (!card.LastAnsweredAt.HasValue) continue;

                // Calculate difficulty based on study progress and time
                var difficultyScore = CalculateDifficultyScore(card);
                
                if (difficultyScore > 0) // Only questions with some difficulty
                {
                    difficultQuestions.Add(new DifficultQuestion
                    {
                        QuestionId = question.Id,
                        Question = question,
                        EstimatedAttempts = Math.Max(1, 3 - card.ConsecutiveCorrectCount) + 
                                          (card.LastAnsweredAt.HasValue ? 1 : 0),
                        DifficultyScore = difficultyScore,
                        IsLearned = card.IsLearned,
                        TopicId = question.TopicId,
                        Topic = topic,
                        LastAnsweredAt = card.LastAnsweredAt
                    });
                }
            }
            // Remove the else block that was adding unattempted questions as "difficult"
        }

        return difficultQuestions
            .OrderByDescending(dq => dq.DifficultyScore)
            .ThenByDescending(dq => dq.EstimatedAttempts)
            .Take(10)
            .ToList();
    }

    private int CalculateDifficultyScore(QuestionStudyCard card)
    {
        var score = 0;

        // Base difficulty based on learning progress
        if (card.ConsecutiveCorrectCount == 0)
            score += 3; // Never answered correctly
        else if (card.ConsecutiveCorrectCount == 1)
            score += 2; // Answered correctly only once
        else if (card.ConsecutiveCorrectCount == 2)
            score += 1; // Almost learned, but not yet

        // Additional difficulty based on time
        if (card.LastAnsweredAt.HasValue)
        {
            var daysSinceLastAnswer = (DateTime.Now - card.LastAnsweredAt.Value).TotalDays;
            if (daysSinceLastAnswer > 30)
                score += 2; // Long time without answering
            else if (daysSinceLastAnswer > 7)
                score += 1; // A week without answering
        }

        return score;
    }
}