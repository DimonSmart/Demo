namespace Demo.Demos.Quiz;

internal static class LanguageCodes
{
    public static IReadOnlyList<string> Normalize(IEnumerable<string>? languages)
    {
        if (languages is null)
        {
            return [];
        }

        var uniqueLanguages = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var normalizedLanguages = new List<string>();

        foreach (var language in languages)
        {
            var languageCode = language?.Trim();
            if (string.IsNullOrEmpty(languageCode) || !uniqueLanguages.Add(languageCode))
            {
                continue;
            }

            normalizedLanguages.Add(languageCode);
        }

        return normalizedLanguages;
    }

    public static string SelectPrimary(IReadOnlyList<string> languages, string? savedLanguage)
    {
        return languages.FirstOrDefault(language => string.Equals(language, savedLanguage, StringComparison.OrdinalIgnoreCase))
            ?? languages.FirstOrDefault()
            ?? string.Empty;
    }
}
