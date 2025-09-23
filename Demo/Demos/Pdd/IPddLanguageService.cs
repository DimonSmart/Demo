namespace Demo.Demos.Pdd;

/// <summary>
/// Service for managing PDD language preferences and localization
/// </summary>
public interface IPddLanguageService
    {
        /// <summary>
        /// Gets the current primary language for PDD questions
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