using Demo.Demos.Common;

using Demo.Core.Quiz;

namespace Demo.Demos.Quiz;

/// <summary>
/// Service for managing quiz language preferences and providing localized content.
/// </summary>
public class QuizLanguageService(QuizPreferencesService preferencesStorage) : IQuizLanguageService
{
    /// <summary>
    /// Gets the current primary language for quiz questions from user preferences.
    /// </summary>
    /// <returns>Language code (e.g., "es", "ru", "en")</returns>
    public async Task<string> GetPrimaryLanguageAsync()
    {
        var preferences = await preferencesStorage.LoadAsync();
        return preferences.PrimaryLanguage;
    }

        /// <summary>
        /// Gets localized text content based on the current primary language
        /// </summary>
        /// <param name="localizedText">The localized text object</param>
        /// <param name="primaryLanguage">Primary language code (e.g., "es", "ru", "en")</param>
        /// <param name="fallbackText">Text to return if no suitable localization is found</param>
        /// <returns>Localized text string</returns>
        public string GetLocalizedContent(LocalizedText localizedText, string primaryLanguage, string fallbackText = "")
        {
            return localizedText.Get(primaryLanguage) is { Length: > 0 } content ? content : fallbackText;
        }
    }
