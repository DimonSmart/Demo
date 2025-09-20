using System.Collections.Generic;

namespace Demo.Demos.MarkdownToWord;

internal static class ConfusableCharacterDefinitions
{
    internal static readonly IReadOnlyDictionary<int, string> Punctuation = new Dictionary<int, string>
    {
        { 0x201C, "\"" }, // LEFT DOUBLE QUOTATION MARK → ASCII
        { 0x201D, "\"" }, // RIGHT DOUBLE QUOTATION MARK → ASCII
        { 0x2018, "'" },   // LEFT SINGLE QUOTATION MARK → ASCII
        { 0x2019, "'" },   // RIGHT SINGLE QUOTATION MARK → ASCII
        { 0x2212, "-" },   // MINUS SIGN → HYPHEN-MINUS
        { 0x2013, "-" },   // EN DASH → HYPHEN-MINUS
        { 0x2014, "-" }    // EM DASH → HYPHEN-MINUS
    };

    internal static readonly IReadOnlyDictionary<int, ConfusableLetterDefinition> Letters = new Dictionary<int, ConfusableLetterDefinition>
    {
        { 0x0430, new ConfusableLetterDefinition("a", true) }, // CYRILLIC SMALL LETTER A → LATIN
        { 0x0435, new ConfusableLetterDefinition("e", true) }, // CYRILLIC SMALL LETTER IE → LATIN
        { 0x043E, new ConfusableLetterDefinition("o", true) }, // CYRILLIC SMALL LETTER O → LATIN
        { 0x0440, new ConfusableLetterDefinition("p", true) }, // CYRILLIC SMALL LETTER ER → LATIN
        { 0x0441, new ConfusableLetterDefinition("c", true) }, // CYRILLIC SMALL LETTER ES → LATIN
        { 0x0443, new ConfusableLetterDefinition("y", true) }, // CYRILLIC SMALL LETTER U → LATIN
        { 0x0445, new ConfusableLetterDefinition("x", true) }  // CYRILLIC SMALL LETTER HA → LATIN
    };

    internal static bool TryGetReplacement(int codePoint, out string replacement)
    {
        if (Punctuation.TryGetValue(codePoint, out var punctuationReplacement))
        {
            replacement = punctuationReplacement;
            return true;
        }

        if (Letters.TryGetValue(codePoint, out var letterDefinition))
        {
            replacement = letterDefinition.Replacement;
            return true;
        }

        replacement = string.Empty;
        return false;
    }
}

internal readonly record struct ConfusableLetterDefinition(string Replacement, bool RequiresLatinContext);
