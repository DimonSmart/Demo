using Blazored.LocalStorage;
using System.Globalization;
using System.Text.Json;

namespace Demo.Demos.Quiz;

public sealed class CardStorageService(ILocalStorageService localStorage, ILogger<CardStorageService> logger)
{
    private const string LegacyStorageKey = "questionCards";

    public async Task SaveCardsAsync(StoredCardsSet data)
    {
        await localStorage.SetItemAsync(GetStorageKey(data.QuizDocumentId), data.ToCompact());
    }

    public async Task<StoredCardsSet?> LoadCardsAsync(string quizDocumentId, IReadOnlySet<string> currentQuestionIds)
    {
        await TryMigrateLegacyAsync(quizDocumentId);
        var compact = await localStorage.GetItemAsync<StoredCardsSetCompact>(GetStorageKey(quizDocumentId));
        if (compact is null) return null;

        var domain = compact.ToDomain();
        if (!string.Equals(domain.QuizDocumentId, quizDocumentId, StringComparison.Ordinal)) return null;
        return SynchronizeWithDocument(domain, currentQuestionIds);
    }

    public async Task ResetProgressAsync(string quizDocumentId)
    {
        await localStorage.RemoveItemAsync(GetStorageKey(quizDocumentId));
    }

    private async Task TryMigrateLegacyAsync(string quizDocumentId)
    {
        if (await localStorage.ContainKeyAsync(GetStorageKey(quizDocumentId))) return;

        var raw = await localStorage.GetItemAsStringAsync(LegacyStorageKey);
        if (string.IsNullOrWhiteSpace(raw)) return;

        try
        {
            using var document = JsonDocument.Parse(raw);
            await SaveCardsAsync(new StoredCardsSet
            {
                QuizDocumentId = quizDocumentId,
                Cards = ReadLegacyCards(document.RootElement)
            });
            await localStorage.RemoveItemAsync(LegacyStorageKey);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Unable to migrate legacy quiz progress. Progress will be reset.");
            await localStorage.RemoveItemAsync(LegacyStorageKey);
        }
    }

    private static List<QuestionStudyCard> ReadLegacyCards(JsonElement root)
    {
        var cardsElement = root.ValueKind == JsonValueKind.Array
            ? root
            : root.TryGetProperty("C", out var compactCards)
                ? compactCards
                : default;
        if (cardsElement.ValueKind != JsonValueKind.Array) return [];

        var cards = new List<QuestionStudyCard>();
        foreach (var item in cardsElement.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.Array || item.GetArrayLength() == 0) continue;

            var id = item[0].ValueKind == JsonValueKind.Number
                ? item[0].GetInt32().ToString(CultureInfo.InvariantCulture)
                : item[0].GetString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(id)) continue;

            var count = 0;
            if (item.GetArrayLength() > 1)
            {
                count = item[1].ValueKind == JsonValueKind.Number
                    ? item[1].GetInt32()
                    : int.TryParse(item[1].GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
                        ? parsed
                        : 0;
            }

            cards.Add(new QuestionStudyCard { Id = id, ConsecutiveCorrectCount = count });
        }

        return cards;
    }

    private static StoredCardsSet SynchronizeWithDocument(StoredCardsSet stored, IReadOnlySet<string> currentQuestionIds)
    {
        var cardsById = stored.Cards
            .Where(card => currentQuestionIds.Contains(card.Id))
            .GroupBy(card => card.Id, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);

        foreach (var questionId in currentQuestionIds)
        {
            cardsById.TryAdd(questionId, new QuestionStudyCard { Id = questionId });
        }

        return new StoredCardsSet
        {
            QuizDocumentId = stored.QuizDocumentId,
            Cards = cardsById.Values.ToArray()
        };
    }

    private static string GetStorageKey(string quizDocumentId) => $"quizProgress:{quizDocumentId}";
}
