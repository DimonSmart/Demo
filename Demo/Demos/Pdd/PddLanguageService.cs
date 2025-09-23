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
        return preferences?.PrimaryLanguage ?? "en";
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
            if (localizedText == null) return fallbackText;

            // Try primary language first
            switch (primaryLanguage?.ToLower())
            {
                case "ru":
                case "russian":
                    if (!string.IsNullOrEmpty(localizedText.Russian))
                        return localizedText.Russian;
                    break;
                case "es":
                case "spanish":
                    if (!string.IsNullOrEmpty(localizedText.Spanish))
                        return localizedText.Spanish;
                    break;
                case "en":
                case "english":
                    if (!string.IsNullOrEmpty(localizedText.English))
                        return localizedText.English;
                    break;
            }

            // Fallback sequence: Russian -> Spanish -> English
            if (!string.IsNullOrEmpty(localizedText.Russian))
                return localizedText.Russian;
            if (!string.IsNullOrEmpty(localizedText.Spanish))
                return localizedText.Spanish;
            if (!string.IsNullOrEmpty(localizedText.English))
                return localizedText.English;

            return fallbackText;
        }
    }