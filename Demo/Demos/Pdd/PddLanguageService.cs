using Demo.Demos.Common;

namespace Demo.Demos.Pdd;

/// <summary>
/// Service for managing PDD language preferences and providing localized content
/// </summary>
public class PddLanguageService(UserPreferencesStorageService<PddUserPreferences> preferencesStorage) : IPddLanguageService
{
    /// <summary>
    /// Gets the current primary language for PDD questions from user preferences
    /// </summary>
    /// <returns>Language code (e.g., "es", "ru", "en")</returns>
    public async Task<string> GetPrimaryLanguageAsync()
    {
        var preferences = await preferencesStorage.LoadPreferencesAsync();
        return preferences?.PrimaryLanguage ?? string.Empty;
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
