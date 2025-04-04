namespace Demo.Demos.Pdd;

/// <summary>
/// Manages the queue of study cards and processes answers.
/// Questions remain in the queue until they are learned (answered correctly 3 times in a row).
/// Questions are reordered based on answer correctness to optimize learning.
/// </summary>
public class QuestionQueue
{
    private readonly List<QuestionStudyCard> _cards;
    private readonly Random _random = new();
    private const int TicketSize = 30;

    private readonly int _initialCount;
    private int _learnedCount;
    private int _correctAnswers;
    private int _incorrectAnswers;

    public QuestionQueue(IEnumerable<QuestionStudyCard> cards)
    {
        var allCards = cards.ToList();
        _initialCount = allCards.Count;

        // Count how many were already learned
        _learnedCount = allCards.Count(c => c.IsLearned);

        // Store only those that are not learned yet
        _cards = allCards.Where(c => !c.IsLearned).ToList();
    }

    /// <summary>
    /// The total number of questions (initially loaded).
    /// </summary>
    public int TotalQuestionCount => _initialCount;

    /// <summary>
    /// How many questions are learned (excluded from the queue).
    /// </summary>
    public int LearnedQuestionCount => _learnedCount;

    public IReadOnlyCollection<QuestionStudyCard> Cards => _cards.AsReadOnly();

    /// <summary>
    /// Gets the first N available questions that are not marked as learned.
    /// This method does not remove questions from the queue - it just returns them for study.
    /// Questions are only reordered within the queue when ProcessAnswer is called.
    /// </summary>
    /// <param name="count">The number of questions to peek from the queue.</param>
    /// <returns>A list of study cards for the user to answer, may be smaller than requested count if not enough questions are available.</returns>
    public List<QuestionStudyCard> PeekNextQuestionsForStudy(int count)
    {
        return [.. _cards.Where(c => !c.IsLearned).Take(count)];
    }

    /// <summary>
    /// Processes a user's answer to a question and updates the queue accordingly.
    /// If the answer is correct:
    /// - Increments the consecutive correct count
    /// - If answered correctly 3 times, removes from queue and marks as learned
    /// - Otherwise, moves the question further back in the queue to be reviewed later
    /// If the answer is incorrect:
    /// - Resets consecutive correct count
    /// - Moves the question back to the front portion of the queue for quick review
    /// </summary>
    public void ProcessAnswer(QuestionStudyCard card, bool isCorrect)
    {
        if (card == null) return;

        if (!isCorrect)
        {
            _incorrectAnswers++;
            card.ConsecutiveCorrectCount = 0;
            RequeueQuestion(card, false);
            return;
        }

        _correctAnswers++;
        card.ConsecutiveCorrectCount++;
        if (card.ConsecutiveCorrectCount < 3)
        {
            RequeueQuestion(card, true);
            return;
        }

        // Once a card is learned, remove it from the queue
        var idx = _cards.FindIndex(c => c == card);
        if (idx >= 0)
        {
            _cards.RemoveAt(idx);
            _learnedCount++;
        }
    }

    /// <summary>
    /// Gets the current learning metrics including progress statistics and answer history
    /// </summary>
    public LearningMetrics GetMetrics()
    {
        return new LearningMetrics
        {
            TotalQuestions = _initialCount,
            LearnedQuestions = _learnedCount,
            InProgressQuestions = _cards.Count(c => c.ConsecutiveCorrectCount > 0),
            NotStartedQuestions = _cards.Count(c => c.ConsecutiveCorrectCount == 0),
            CorrectAnswers = _correctAnswers,
            IncorrectAnswers = _incorrectAnswers
        };
    }

    /// <summary>
    /// Requeues a question within the queue based on the answer correctness.
    /// For correct answers (placeNearEnd=true): places the question near the end of the queue
    /// For incorrect answers (placeNearEnd=false): places the question within the first portion (TicketSize) of the queue
    /// </summary>
    private void RequeueQuestion(QuestionStudyCard card, bool placeNearEnd)
    {
        var index = _cards.FindIndex(c => c == card);
        if (index < 0) return;

        _cards.RemoveAt(index);

        if (_cards.Count < TicketSize)
        {
            _cards.Add(card);
            return;
        }

        if (!placeNearEnd)
        {
            var insertIndex = Math.Min(TicketSize, _cards.Count);
            _cards.Insert(insertIndex, card);
            return;
        }

        var randomIndex = _random.Next(TicketSize, _cards.Count + 1);
        _cards.Insert(randomIndex, card);
    }
}
