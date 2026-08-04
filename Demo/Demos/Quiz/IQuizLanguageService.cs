using Demo.Core.Quiz;

namespace Demo.Demos.Quiz;

/// <summary>
/// Service for managing quiz language preferences and localization.
/// </summary>
public interface IQuizLanguageService
    {
        /// <summary>
        /// Gets the current primary language for quiz questions.
        /// </summary>
        /// <returns>Language code (e.g., "es", "ru", "en")</returns>
        Task<string> GetPrimaryLanguageAsync();

        /// <summary>
        /// Gets localized text content based on a specific language
        /// </summary>
        /// <param name="localizedText">The localized text object</param>
        /// <param name="primaryLanguage">Primary language code (e.g., "es", "ru", "en")</param>
        /// <param name="fallbackText">Text to return if no suitable localization is found</param>
        /// <returns>Localized text string</returns>
        string GetLocalizedContent(LocalizedText localizedText, string primaryLanguage, string fallbackText = "");
    }
