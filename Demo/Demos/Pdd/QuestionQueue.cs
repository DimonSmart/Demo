namespace Demo.Demos.Pdd;

/// <summary>
/// Manages the queue of study cards and processes answers.
/// </summary>
public class QuestionQueue
{
    private readonly List<QuestionStudyCard> _cards;
    private readonly Random _random = new();
    private const int TicketSize = 30;

    private readonly int _initialCount;
    private int _learnedCount;

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
    /// Gets the next available question that is not marked as learned.
    /// </summary>
    public QuestionStudyCard? GetNextQuestion()
    {
        return _cards.FirstOrDefault(c => !c.IsLearned);
    }

    /// <summary>
    /// Gets the specified number of next available questions that are not marked as learned.
    /// </summary>
    /// <param name="count">The number of questions to retrieve.</param>
    /// <returns>A list of study cards, may be smaller than requested count if not enough questions are available.</returns>
    public List<QuestionStudyCard> GetNextQuestion(int count)
    {
        return _cards.Where(c => !c.IsLearned).Take(count).ToList();
    }

    public void ProcessAnswer(QuestionStudyCard card, bool isCorrect)
    {
        if (card == null) return;

        if (!isCorrect)
        {
            card.ConsecutiveCorrectCount = 0;
            RequeueQuestion(card, false);
            return;
        }

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
