using System.Globalization;

namespace Demo.Demos.Pdd;

public static class StoredCardsSetExtensions
{
    public static StoredCardsSetCompact ToCompact(this StoredCardsSet domain)
    {
        var cards = domain.Cards
            .Select(card => new[]
            {
                card.Id,
                card.ConsecutiveCorrectCount.ToString(CultureInfo.InvariantCulture)
            })
            .ToList();

        return new StoredCardsSetCompact(domain.FormatVersion, domain.QuizDocumentId, cards);
    }

    public static StoredCardsSet ToDomain(this StoredCardsSetCompact compact)
    {
        if (!string.Equals(compact.FormatVersion, "2.0", StringComparison.Ordinal))
        {
            return new StoredCardsSet { QuizDocumentId = compact.QuizDocumentId };
        }

        var cards = compact.Cards
            .Where(card => card.Length > 0)
            .Select(card => new QuestionStudyCard
            {
                Id = card[0],
                ConsecutiveCorrectCount = card.Length > 1 && int.TryParse(card[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var count) ? count : 0
            })
            .ToArray();

        return new StoredCardsSet
        {
            FormatVersion = compact.FormatVersion,
            QuizDocumentId = compact.QuizDocumentId,
            Cards = cards
        };
    }
}
