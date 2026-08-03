using Blazored.LocalStorage;
using Demo.Core.Quiz;

namespace Demo.Demos.Pdd;
public sealed class QuizProgressService(ILocalStorageService localStorage)
{
    public async Task<StoredQuizProgress?> LoadAsync(string sourceId) => await localStorage.GetItemAsync<StoredQuizProgress>($"quizProgress:v3:{sourceId}");
    public async Task ResetAsync(string sourceId) => await localStorage.RemoveItemAsync($"quizProgress:v3:{sourceId}");
    public async Task<(StoredQuizProgress Progress, QuizSynchronizationResult Result)> SynchronizeAsync(LoadedQuizDocument loaded)
    {
        var existing = await LoadAsync(loaded.Source.SourceId) ?? await MigrateBuiltInAsync(loaded) ?? new StoredQuizProgress { SourceId = loaded.Source.SourceId };
        var now = DateTime.UtcNow; var result = new QuizSynchronizationResult();
        var stored = existing.Questions.ToList(); var currentHashes = loaded.QuestionHashes;
        var exact = stored.ToDictionary(x => (x.QuestionId, x.ContentHash));
        var currentHashCounts = currentHashes.Values.GroupBy(x => x).ToDictionary(x => x.Key, x => x.Count());
        var storedHashCounts = stored.GroupBy(x => x.ContentHash).ToDictionary(x => x.Key, x => x.Count());
        var active = new List<StoredQuestionProgress>(); int preserved=0, reset=0, added=0, reidentified=0;
        foreach (var question in loaded.Document.Questions)
        {
            var hash = currentHashes[question.Id];
            if (exact.TryGetValue((question.Id, hash), out var found)) { active.Add(Copy(found, question.Id, hash, found.FirstSeenAtUtc, now)); preserved++; continue; }
            if (existing.LastDocumentId == loaded.Document.Id && stored.Any(x => x.QuestionId == question.Id)) { active.Add(New(question.Id, hash, now)); reset++; continue; }
            var matches = stored.Where(x => x.ContentHash == hash).ToArray();
            if (matches.Length == 1 && currentHashCounts[hash] == 1 && storedHashCounts[hash] == 1) { active.Add(Copy(matches[0], question.Id, hash, matches[0].FirstSeenAtUtc, now)); reidentified++; continue; }
            active.Add(New(question.Id, hash, now)); added++;
        }
        var activeKeys = active.Select(x => (x.QuestionId, x.ContentHash)).ToHashSet();
        var archived = stored.Where(x => !activeKeys.Contains((x.QuestionId, x.ContentHash))).ToList();
        var progress = new StoredQuizProgress { SourceId = loaded.Source.SourceId, LastDocumentId = loaded.Document.Id, LastDocumentHash = loaded.DocumentHash, Questions = [.. archived, .. active] };
        await localStorage.SetItemAsync($"quizProgress:v3:{loaded.Source.SourceId}", progress);
        return (progress, new QuizSynchronizationResult { PreservedQuestions=preserved, ResetQuestions=reset, AddedQuestions=added, RemovedQuestions=archived.Count, ReidentifiedQuestions=reidentified });
    }
    private async Task<StoredQuizProgress?> MigrateBuiltInAsync(LoadedQuizDocument loaded)
    {
        if (!loaded.Source.IsBuiltIn) return null;
        var legacy = await localStorage.GetItemAsync<StoredCardsSetCompact>($"quizProgress:{loaded.Document.Id}");
        if (legacy is null) return null;
        var cards = legacy.ToDomain().Cards.ToDictionary(x => x.Id, StringComparer.Ordinal);
        var now = DateTime.UtcNow;
        var migrated = new StoredQuizProgress { SourceId = loaded.Source.SourceId, LastDocumentId = loaded.Document.Id, LastDocumentHash = loaded.DocumentHash, Questions = loaded.Document.Questions.Select(q =>
        {
            cards.TryGetValue(q.Id, out var card);
            return new StoredQuestionProgress { QuestionId=q.Id, ContentHash=loaded.QuestionHashes[q.Id], ConsecutiveCorrectCount=card?.ConsecutiveCorrectCount ?? 0, LastAnsweredAt=card?.LastAnsweredAt, FirstSeenAtUtc=now, LastSeenAtUtc=now };
        }).ToArray() };
        await localStorage.SetItemAsync($"quizProgress:v3:{loaded.Source.SourceId}", migrated);
        await localStorage.RemoveItemAsync($"quizProgress:{loaded.Document.Id}");
        return migrated;
    }
    public async Task SaveCardsAsync(LoadedQuizDocument loaded, IEnumerable<QuestionStudyCard> cards)
    {
        var progress = await LoadAsync(loaded.Source.SourceId) ?? new StoredQuizProgress { SourceId = loaded.Source.SourceId };
        var byId = cards.ToDictionary(x => x.Id); var now = DateTime.UtcNow;
        var questions = progress.Questions.Select(x => byId.TryGetValue(x.QuestionId, out var card) && loaded.QuestionHashes.TryGetValue(x.QuestionId, out var hash) && hash == x.ContentHash ? Copy(x,x.QuestionId,x.ContentHash,x.FirstSeenAtUtc,now,card) : x).ToArray();
        await localStorage.SetItemAsync($"quizProgress:v3:{loaded.Source.SourceId}", new StoredQuizProgress { SourceId=loaded.Source.SourceId, LastDocumentId=loaded.Document.Id, LastDocumentHash=loaded.DocumentHash, Questions=questions });
    }
    public static IReadOnlyCollection<QuestionStudyCard> ActiveCards(LoadedQuizDocument loaded, StoredQuizProgress progress) => loaded.Document.Questions.Select(q => { var p=progress.Questions.First(x => x.QuestionId == q.Id && x.ContentHash == loaded.QuestionHashes[q.Id]); return new QuestionStudyCard { Id=q.Id, ConsecutiveCorrectCount=p.ConsecutiveCorrectCount, LastAnsweredAt=p.LastAnsweredAt }; }).ToArray();
    private static StoredQuestionProgress New(string id,string hash,DateTime now) => new(){QuestionId=id,ContentHash=hash,FirstSeenAtUtc=now,LastSeenAtUtc=now};
    private static StoredQuestionProgress Copy(StoredQuestionProgress p,string id,string hash,DateTime first,DateTime seen,QuestionStudyCard? card=null) => new(){QuestionId=id,ContentHash=hash,ConsecutiveCorrectCount=card?.ConsecutiveCorrectCount ?? p.ConsecutiveCorrectCount,LastAnsweredAt=card?.LastAnsweredAt ?? p.LastAnsweredAt,FirstSeenAtUtc=first,LastSeenAtUtc=seen};
}
