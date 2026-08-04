using Blazored.LocalStorage;

namespace Demo.Demos.Quiz;

public sealed class QuizPreferencesService(ILocalStorageService localStorage)
{
    private const string StorageKey = "quiz.userPreferences";
    private const string LegacyStorageKey = "userPreferences";

    public async Task<QuizUserPreferences> LoadAsync()
    {
        var preferences = await localStorage.GetItemAsync<QuizUserPreferences>(StorageKey);
        if (preferences is not null) return preferences;

        var legacy = await localStorage.GetItemAsync<QuizUserPreferences>(LegacyStorageKey);
        preferences = legacy is null
            ? new QuizUserPreferences()
            : new QuizUserPreferences { PrimaryLanguage = legacy.PrimaryLanguage, HighlightTerms = legacy.HighlightTerms };
        await localStorage.SetItemAsync(StorageKey, preferences);
        return preferences;
    }

    public async Task UpdateAsync(Action<QuizUserPreferences> update)
    {
        var preferences = await LoadAsync();
        update(preferences);
        await localStorage.SetItemAsync(StorageKey, preferences);
    }
}
