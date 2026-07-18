namespace Demo.Core.Quiz;

public static class Bcp47LanguageTag
{
    public static bool IsValidCanonical(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag) || tag.Length > 64) return false;
        if (tag.Contains('_', StringComparison.Ordinal)) return false;

        var parts = tag.Split('-');
        if (parts.Any(string.IsNullOrEmpty)) return false;

        var index = 0;
        if (parts[index] == "x")
        {
            return ValidatePrivateUse(parts, index + 1);
        }

        if (parts[index].Length is < 2 or > 3 || !IsLowerAlpha(parts[index])) return false;
        index++;

        if (index < parts.Length && parts[index].Length == 4)
        {
            if (!IsTitleAlpha(parts[index])) return false;
            index++;
        }

        if (index < parts.Length && (parts[index].Length == 2 || parts[index].Length == 3))
        {
            if (parts[index].Length == 2 && !IsUpperAlpha(parts[index])) return false;
            if (parts[index].Length == 3 && !parts[index].All(char.IsDigit)) return false;
            index++;
        }

        while (index < parts.Length && IsVariant(parts[index]))
        {
            index++;
        }

        while (index < parts.Length && IsExtensionSingleton(parts[index]))
        {
            index++;
            var extensionStart = index;
            while (index < parts.Length && parts[index].Length is >= 2 and <= 8 && IsLowerAlnum(parts[index]))
            {
                index++;
            }

            if (index == extensionStart) return false;
        }

        if (index < parts.Length && parts[index] == "x")
        {
            return ValidatePrivateUse(parts, index + 1);
        }

        return index == parts.Length;
    }

    private static bool ValidatePrivateUse(string[] parts, int start) =>
        start < parts.Length && parts.Skip(start).All(part => part.Length is >= 1 and <= 8 && IsLowerAlnum(part));

    private static bool IsVariant(string value) =>
        value.Length is >= 5 and <= 8 && IsLowerAlnum(value)
        || value.Length == 4 && char.IsDigit(value[0]) && IsLowerAlnum(value);

    private static bool IsExtensionSingleton(string value) =>
        value.Length == 1 && value is not "x" && (value[0] is >= 'a' and <= 'z' or >= '0' and <= '9');

    private static bool IsLowerAlpha(string value) => value.All(c => c is >= 'a' and <= 'z');
    private static bool IsUpperAlpha(string value) => value.All(c => c is >= 'A' and <= 'Z');
    private static bool IsLowerAlnum(string value) => value.All(c => c is >= 'a' and <= 'z' or >= '0' and <= '9');
    private static bool IsTitleAlpha(string value) => value[0] is >= 'A' and <= 'Z' && value.Skip(1).All(c => c is >= 'a' and <= 'z');
}
