namespace Demo.Demos.Pdd;

public static class StoredCardsSetExtensions
{
    private const string CURRENT_VERSION = "1.0";

    // Domain -> Compact
    public static StoredCardsSetCompact ToCompact(this StoredCardsSet domain)
    {
        // Convert each card to [id, consecutiveCorrectCount]
        var list = domain.Cards
            .Select(c => new[] { c.Id, c.ConsecutiveCorrectCount.ToString(System.Globalization.CultureInfo.InvariantCulture) })
            .ToList();

        return new StoredCardsSetCompact(domain.Version, list);
    }

    // Compact -> Domain (with version check)
    public static StoredCardsSet ToDomain(this StoredCardsSetCompact compact)
    {
        if (!string.Equals(compact.V, CURRENT_VERSION, StringComparison.Ordinal))
        {
            return new StoredCardsSet
            {
                Version = CURRENT_VERSION,
                Cards = Array.Empty<QuestionStudyCard>()
            };
        }

        var domainCards = compact.C
            .Select(arr => new QuestionStudyCard
            {
                Id = arr[0],
                ConsecutiveCorrectCount = int.Parse(arr[1], System.Globalization.CultureInfo.InvariantCulture)
            })
            .ToArray();

        return new StoredCardsSet
        {
            Version = compact.V,
            Cards = domainCards
        };
    }
}
