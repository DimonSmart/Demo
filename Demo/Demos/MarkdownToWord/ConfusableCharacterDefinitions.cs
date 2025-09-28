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
        { 0x0430, new ConfusableLetterDefinition("a", ConfusableLetterContext.Latin) }, // CYRILLIC SMALL LETTER A → LATIN
        { 0x0435, new ConfusableLetterDefinition("e", ConfusableLetterContext.Latin) }, // CYRILLIC SMALL LETTER IE → LATIN
        { 0x0438, new ConfusableLetterDefinition("u", ConfusableLetterContext.Latin) }, // CYRILLIC SMALL LETTER I → LATIN
        { 0x043E, new ConfusableLetterDefinition("o", ConfusableLetterContext.Latin) }, // CYRILLIC SMALL LETTER O → LATIN
        { 0x0440, new ConfusableLetterDefinition("p", ConfusableLetterContext.Latin) }, // CYRILLIC SMALL LETTER ER → LATIN
        { 0x0441, new ConfusableLetterDefinition("c", ConfusableLetterContext.Latin) }, // CYRILLIC SMALL LETTER ES → LATIN
        { 0x0443, new ConfusableLetterDefinition("y", ConfusableLetterContext.Latin) }, // CYRILLIC SMALL LETTER U → LATIN
        { 0x0445, new ConfusableLetterDefinition("x", ConfusableLetterContext.Latin) }, // CYRILLIC SMALL LETTER HA → LATIN
        { 0x0410, new ConfusableLetterDefinition("A", ConfusableLetterContext.Latin) }, // CYRILLIC CAPITAL LETTER A → LATIN
        { 0x0415, new ConfusableLetterDefinition("E", ConfusableLetterContext.Latin) }, // CYRILLIC CAPITAL LETTER IE → LATIN
        { 0x041E, new ConfusableLetterDefinition("O", ConfusableLetterContext.Latin) }, // CYRILLIC CAPITAL LETTER O → LATIN
        { 0x0420, new ConfusableLetterDefinition("P", ConfusableLetterContext.Latin) }, // CYRILLIC CAPITAL LETTER ER → LATIN
        { 0x0421, new ConfusableLetterDefinition("C", ConfusableLetterContext.Latin) }, // CYRILLIC CAPITAL LETTER ES → LATIN
        { 0x0423, new ConfusableLetterDefinition("Y", ConfusableLetterContext.Latin) }, // CYRILLIC CAPITAL LETTER U → LATIN
        { 0x0425, new ConfusableLetterDefinition("X", ConfusableLetterContext.Latin) }, // CYRILLIC CAPITAL LETTER HA → LATIN
        { 0x0061, new ConfusableLetterDefinition("а", ConfusableLetterContext.Cyrillic) }, // LATIN SMALL LETTER A → CYRILLIC
        { 0x0065, new ConfusableLetterDefinition("е", ConfusableLetterContext.Cyrillic) }, // LATIN SMALL LETTER E → CYRILLIC
        { 0x006F, new ConfusableLetterDefinition("о", ConfusableLetterContext.Cyrillic) }, // LATIN SMALL LETTER O → CYRILLIC
        { 0x0070, new ConfusableLetterDefinition("р", ConfusableLetterContext.Cyrillic) }, // LATIN SMALL LETTER P → CYRILLIC
        { 0x0073, new ConfusableLetterDefinition("ѕ", ConfusableLetterContext.Cyrillic) }, // LATIN SMALL LETTER S → CYRILLIC
        { 0x0063, new ConfusableLetterDefinition("с", ConfusableLetterContext.Cyrillic) }, // LATIN SMALL LETTER C → CYRILLIC
        { 0x0078, new ConfusableLetterDefinition("х", ConfusableLetterContext.Cyrillic) }, // LATIN SMALL LETTER X → CYRILLIC
        { 0x0079, new ConfusableLetterDefinition("у", ConfusableLetterContext.Cyrillic) }, // LATIN SMALL LETTER Y → CYRILLIC
        { 0x0041, new ConfusableLetterDefinition("А", ConfusableLetterContext.Cyrillic) }, // LATIN CAPITAL LETTER A → CYRILLIC
        { 0x0045, new ConfusableLetterDefinition("Е", ConfusableLetterContext.Cyrillic) }, // LATIN CAPITAL LETTER E → CYRILLIC
        { 0x004F, new ConfusableLetterDefinition("О", ConfusableLetterContext.Cyrillic) }, // LATIN CAPITAL LETTER O → CYRILLIC
        { 0x0050, new ConfusableLetterDefinition("Р", ConfusableLetterContext.Cyrillic) }, // LATIN CAPITAL LETTER P → CYRILLIC
        { 0x0053, new ConfusableLetterDefinition("Ѕ", ConfusableLetterContext.Cyrillic) }, // LATIN CAPITAL LETTER S → CYRILLIC
        { 0x0043, new ConfusableLetterDefinition("С", ConfusableLetterContext.Cyrillic) }, // LATIN CAPITAL LETTER C → CYRILLIC
        { 0x0058, new ConfusableLetterDefinition("Х", ConfusableLetterContext.Cyrillic) }, // LATIN CAPITAL LETTER X → CYRILLIC
        { 0x0059, new ConfusableLetterDefinition("У", ConfusableLetterContext.Cyrillic) }  // LATIN CAPITAL LETTER Y → CYRILLIC
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

internal enum ConfusableLetterContext
{
    None,
    Latin,
    Cyrillic
}

internal readonly record struct ConfusableLetterDefinition(string Replacement, ConfusableLetterContext RequiredContext);
