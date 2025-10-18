using System.Globalization;
using System.Text;

namespace MarkdownToWordFeature.Services
{
    /// <summary>
    /// Advanced invisible character cleaner using System.Text.Rune for proper Unicode handling
    /// </summary>
    public static class InvisibleCleaner
    {
        public sealed record Options(
            bool RemoveControlChars = true,
            bool KeepCrLf = true,
            int TabToSpaces = 4,
            bool PreserveZWJZWNJ = false,
            bool InvisibleMathToSpace = false,  // Changed default to false
            bool NormalizeUnicode = true,
            HashSet<int>? WhitelistedCharacters = null,
            HashSet<int>? BlacklistedCharacters = null
        );

        /// <summary>
        /// Clean invisible characters from text with comprehensive Unicode support
        /// </summary>
        public static string Clean(string input, Options? options = null)
        {
            options ??= new Options();

            if (string.IsNullOrEmpty(input))
                return input ?? string.Empty;

            // 1) NFC normalization if enabled
            var text = options.NormalizeUnicode 
                ? input.Normalize(NormalizationForm.FormC) 
                : input;

            // 2) Process using Rune enumeration for proper Unicode handling
            var sb = new StringBuilder(text.Length);
            foreach (var rune in text.EnumerateRunes())
            {
                ProcessRune(rune, sb, options);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Process a single Rune according to cleaning options
        /// </summary>
        private static void ProcessRune(Rune rune, StringBuilder sb, Options options)
        {
            var codePoint = rune.Value;

            // Check whitelist/blacklist first
            if (options.WhitelistedCharacters?.Contains(codePoint) == true)
            {
                sb.Append(rune.ToString());
                return;
            }

            if (options.BlacklistedCharacters?.Contains(codePoint) == true)
            {
                return; // Remove blacklisted characters
            }

            // Special handling for specific characters
            switch (codePoint)
            {
                case '\t':
                    if (options.TabToSpaces > 0)
                        sb.Append(new string(' ', options.TabToSpaces));
                    else
                        sb.Append('\t');
                    return;

                case '\r':
                case '\n':
                    if (!options.KeepCrLf && options.RemoveControlChars)
                        return;
                    sb.Append(rune.ToString());
                    return;

                case 0x200C: // ZWNJ
                case 0x200D: // ZWJ
                    if (options.PreserveZWJZWNJ)
                    {
                        sb.Append(rune.ToString());
                        return;
                    }
                    break; // Fall through to removal logic
            }

            // Determine if character should be removed
            if (ShouldRemove(rune, options))
                return;

            // Check if it's a weird space that should be normalized
            if (IsWeirdSpace(rune, out char replacement))
            {
                sb.Append(replacement);
                return;
            }

            // Handle invisible math operators
            if (IsInvisibleMath(codePoint))
            {
                if (options.InvisibleMathToSpace)
                    sb.Append(' ');
                // If InvisibleMathToSpace is false, they're removed (do nothing)
                return;
            }

            // Normal character - keep it
            sb.Append(rune.ToString());
        }

        /// <summary>
        /// Determine if a Rune should be removed based on cleaning options
        /// </summary>
        private static bool ShouldRemove(Rune rune, Options options)
        {
            var codePoint = rune.Value;

            // C0/C1 control chars (excluding CR/LF/TAB which are handled specially)
            if (options.RemoveControlChars)
            {
                if (codePoint < 0x20 && codePoint is not (0x09 or 0x0A or 0x0D))
                    return true;
                if (codePoint >= 0x7F && codePoint <= 0x9F)
                    return true;
            }

            // Unicode line/paragraph separators (usually unwanted in normal text)
            if (codePoint == 0x2028 || codePoint == 0x2029)
                return true;

            // BiDi/formatting controls
            if (IsBiDiControl(codePoint))
                return true;

            // Zero width no-break space/BOM (when not at start of file)
            if (codePoint == 0xFEFF)
                return true;

            // Zero-width formatting (excluding ZWJ/ZWNJ if preserved)
            if (IsZeroWidthFormat(codePoint))
            {
                if (options.PreserveZWJZWNJ && (codePoint == 0x200C || codePoint == 0x200D))
                    return false;
                return true;
            }

            // Invisible math operators (handled separately above)
            // Removed this check since it's handled above
            // if (IsInvisibleMath(codePoint) && !options.InvisibleMathToSpace)
            //     return true;

            // Variation Selectors
            if (IsVariationSelector(codePoint))
                return true;

            // TAG characters block
            if (IsTagCharacter(codePoint))
                return true;

            // Soft hyphen
            if (codePoint == 0x00AD)
                return true;

            // Combining marks (even when normalization is disabled)
            if (Rune.GetUnicodeCategory(rune) == UnicodeCategory.NonSpacingMark)
                return true;

            return false;
        }

        /// <summary>
        /// Check if character is a non-standard space that should be normalized
        /// </summary>
        private static bool IsWeirdSpace(Rune rune, out char replacement)
        {
            replacement = ' ';
            var codePoint = rune.Value;

            return codePoint is
                0x00A0  // NBSP
                or 0x2000 // EN QUAD
                or 0x2001 // EM QUAD
                or 0x2002 // EN SPACE
                or 0x2003 // EM SPACE
                or 0x2004 // THREE-PER-EM SPACE
                or 0x2005 // FOUR-PER-EM SPACE
                or 0x2006 // SIX-PER-EM SPACE
                or 0x2007 // FIGURE SPACE
                or 0x2008 // PUNCTUATION SPACE
                or 0x2009 // THIN SPACE
                or 0x200A // HAIR SPACE
                or 0x202F // NARROW NO-BREAK SPACE
                or 0x205F // MEDIUM MATHEMATICAL SPACE
                or 0x3000; // IDEOGRAPHIC SPACE
        }

        /// <summary>
        /// Check if character is a BiDi control character
        /// </summary>
        private static bool IsBiDiControl(int codePoint) => codePoint is
            0x200E or 0x200F  // LRM/RLM
            or (>= 0x202A and <= 0x202E)  // LRE/RLE/PDF/LRO/RLO
            or (>= 0x2066 and <= 0x2069); // LRI/RLI/FSI/PDI

        /// <summary>
        /// Check if character is zero-width formatting
        /// </summary>
        private static bool IsZeroWidthFormat(int codePoint) => codePoint is
            0x200B  // ZWSP
            or 0x200C // ZWNJ
            or 0x200D // ZWJ
            or 0x2060 // Word Joiner
            or 0x180E; // MONGOLIAN VOWEL SEPARATOR (historical)

        /// <summary>
        /// Check if character is an invisible math operator
        /// </summary>
        private static bool IsInvisibleMath(int codePoint) => codePoint is
            0x2062  // INVISIBLE TIMES
            or 0x2063 // INVISIBLE SEPARATOR
            or 0x2064; // INVISIBLE PLUS

        /// <summary>
        /// Check if character is a variation selector
        /// </summary>
        private static bool IsVariationSelector(int codePoint) =>
            (codePoint >= 0xFE00 && codePoint <= 0xFE0F) ||
            (codePoint >= 0xE0100 && codePoint <= 0xE01EF);

        /// <summary>
        /// Check if character is from the TAG characters block
        /// </summary>
        private static bool IsTagCharacter(int codePoint) =>
            codePoint >= 0xE0000 && codePoint <= 0xE007F;

        /// <summary>
        /// Get statistics about what was cleaned
        /// </summary>
        public static CleaningStats GetCleaningStats(string original, string cleaned, Options? options = null)
        {
            options ??= new Options();
            var stats = new CleaningStats();

            var originalRunes = original.EnumerateRunes().ToArray();
            var cleanedRunes = cleaned.EnumerateRunes().ToArray();

            stats.OriginalLength = originalRunes.Length;
            stats.CleanedLength = cleanedRunes.Length;
            stats.CharactersRemoved = stats.OriginalLength - stats.CleanedLength;

            // Count removed categories
            var cleanedIndex = 0;
            for (var i = 0; i < originalRunes.Length; i++)
            {
                var originalRune = originalRunes[i];
                
                // If we're past cleaned array or characters don't match, this was removed/replaced
                if (cleanedIndex >= cleanedRunes.Length || 
                    !originalRune.Equals(cleanedRunes[cleanedIndex]))
                {
                    CategorizeRemovedCharacter(originalRune, stats);
                    // Don't increment cleanedIndex - this character was removed
                }
                else
                {
                    cleanedIndex++;
                }
            }

            return stats;
        }

        private static void CategorizeRemovedCharacter(Rune rune, CleaningStats stats)
        {
            var codePoint = rune.Value;

            if (codePoint < 0x20 || (codePoint >= 0x7F && codePoint <= 0x9F))
                stats.ControlCharsRemoved++;
            else if (IsWeirdSpace(rune, out _))
                stats.WeirdSpacesNormalized++;
            else if (IsBiDiControl(codePoint))
                stats.BiDiControlsRemoved++;
            else if (IsZeroWidthFormat(codePoint))
                stats.ZeroWidthRemoved++;
            else if (IsInvisibleMath(codePoint))
                stats.InvisibleMathRemoved++;
            else if (IsVariationSelector(codePoint))
                stats.VariationSelectorsRemoved++;
            else if (IsTagCharacter(codePoint))
                stats.TagCharactersRemoved++;
            else
                stats.OtherRemoved++;
        }
    }

    /// <summary>
    /// Statistics about the cleaning operation
    /// </summary>
    public class CleaningStats
    {
        public int OriginalLength { get; set; }
        public int CleanedLength { get; set; }
        public int CharactersRemoved { get; set; }
        public int ControlCharsRemoved { get; set; }
        public int WeirdSpacesNormalized { get; set; }
        public int BiDiControlsRemoved { get; set; }
        public int ZeroWidthRemoved { get; set; }
        public int InvisibleMathRemoved { get; set; }
        public int VariationSelectorsRemoved { get; set; }
        public int TagCharactersRemoved { get; set; }
        public int OtherRemoved { get; set; }

        public bool HasChanges => CharactersRemoved > 0;

        public string GetSummary()
        {
            if (!HasChanges)
                return "No invisible characters found.";

            var parts = new List<string>();
            if (ControlCharsRemoved > 0) parts.Add($"{ControlCharsRemoved} control chars");
            if (WeirdSpacesNormalized > 0) parts.Add($"{WeirdSpacesNormalized} weird spaces");
            if (BiDiControlsRemoved > 0) parts.Add($"{BiDiControlsRemoved} BiDi controls");
            if (ZeroWidthRemoved > 0) parts.Add($"{ZeroWidthRemoved} zero-width");
            if (InvisibleMathRemoved > 0) parts.Add($"{InvisibleMathRemoved} invisible math");
            if (VariationSelectorsRemoved > 0) parts.Add($"{VariationSelectorsRemoved} variation selectors");
            if (TagCharactersRemoved > 0) parts.Add($"{TagCharactersRemoved} TAG chars");
            if (OtherRemoved > 0) parts.Add($"{OtherRemoved} other");

            return $"Removed/normalized {CharactersRemoved} characters: {string.Join(", ", parts)}.";
        }
    }
}