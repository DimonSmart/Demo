using System.Globalization;

namespace Demo.Demos.MarkdownToWord
{
    /// <summary>
    /// Factory class that creates appropriate CleaningActions based on character category and preset
    /// </summary>
    public static class CleaningActionFactory
    {
        /// <summary>
        /// Creates a dictionary of cleaning actions for different presets based on the character's category and code point
        /// </summary>
        public static Dictionary<CleaningPreset, CleaningAction> CreateActionsForCharacter(InvisibleCharacterCategory category, int codePoint)
        {
            var actions = new Dictionary<CleaningPreset, CleaningAction>();

            switch (category)
            {
                case InvisibleCharacterCategory.C0C1Controls:
                    actions[CleaningPreset.Safe] = CleaningAction.Remove("Remove control characters");
                    actions[CleaningPreset.Aggressive] = CleaningAction.Remove("Remove control characters");
                    actions[CleaningPreset.ASCIIStrict] = CleaningAction.Remove("Remove control characters");
                    actions[CleaningPreset.TypographySoft] = CleaningAction.Remove("Remove control characters");
                    actions[CleaningPreset.RTLSafe] = CleaningAction.Remove("Remove control characters");
                    actions[CleaningPreset.SEOPlain] = CleaningAction.Remove("Remove control characters");
                    break;

                case InvisibleCharacterCategory.LineBreaks:
                    switch (codePoint)
                    {
                        case 0x000D: // CR
                            actions[CleaningPreset.Safe] = CleaningAction.Conditional(ctx => HandleCR(ctx), "Normalize CR");
                            actions[CleaningPreset.Aggressive] = CleaningAction.Replace("\n", "Replace CR with LF");
                            actions[CleaningPreset.ASCIIStrict] = CleaningAction.Replace("\n", "Replace CR with LF");
                            actions[CleaningPreset.TypographySoft] = CleaningAction.Conditional(ctx => HandleCR(ctx), "Normalize CR");
                            actions[CleaningPreset.RTLSafe] = CleaningAction.Conditional(ctx => HandleCR(ctx), "Normalize CR");
                            actions[CleaningPreset.SEOPlain] = CleaningAction.Replace("\n", "Replace CR with LF");
                            break;
                        case 0x0085: // NEL
                        case 0x2028: // LS
                        case 0x2029: // PS
                            actions[CleaningPreset.Safe] = CleaningAction.Replace("\n", "Replace with LF");
                            actions[CleaningPreset.Aggressive] = CleaningAction.Replace("\n", "Replace with LF");
                            actions[CleaningPreset.ASCIIStrict] = CleaningAction.Replace("\n", "Replace with LF");
                            actions[CleaningPreset.TypographySoft] = CleaningAction.Replace("\n", "Replace with LF");
                            actions[CleaningPreset.RTLSafe] = CleaningAction.Replace("\n", "Replace with LF");
                            actions[CleaningPreset.SEOPlain] = CleaningAction.Replace("\n", "Replace with LF");
                            break;
                    }
                    break;

                case InvisibleCharacterCategory.Tab:
                    actions[CleaningPreset.Safe] = CleaningAction.Keep("Keep tabs");
                    actions[CleaningPreset.Aggressive] = CleaningAction.Conditional(ctx => new string(' ', ctx.Options.TabSize), "Replace with spaces");
                    actions[CleaningPreset.ASCIIStrict] = CleaningAction.Keep("Keep tabs for code");
                    actions[CleaningPreset.TypographySoft] = CleaningAction.Replace(" ", "Replace with single space");
                    actions[CleaningPreset.RTLSafe] = CleaningAction.Keep("Keep tabs");
                    actions[CleaningPreset.SEOPlain] = CleaningAction.Replace(" ", "Replace with single space");
                    break;

                case InvisibleCharacterCategory.WideSpaces:
                    actions[CleaningPreset.Safe] = CleaningAction.Replace(" ", "Replace with regular space");
                    actions[CleaningPreset.Aggressive] = CleaningAction.Replace(" ", "Replace with regular space");
                    actions[CleaningPreset.ASCIIStrict] = CleaningAction.Replace(" ", "Replace with regular space");
                    actions[CleaningPreset.TypographySoft] = CleaningAction.Keep("Keep for typography");
                    actions[CleaningPreset.RTLSafe] = CleaningAction.Replace(" ", "Replace with regular space");
                    actions[CleaningPreset.SEOPlain] = CleaningAction.Replace(" ", "Replace with regular space");
                    break;

                case InvisibleCharacterCategory.NoBreakSpaces:
                    actions[CleaningPreset.Safe] = CleaningAction.Keep("Keep for safe mode");
                    actions[CleaningPreset.Aggressive] = CleaningAction.Replace(" ", "Replace with regular space");
                    actions[CleaningPreset.ASCIIStrict] = CleaningAction.Replace(" ", "Replace with regular space");
                    actions[CleaningPreset.TypographySoft] = CleaningAction.Keep("Keep for typography");
                    actions[CleaningPreset.RTLSafe] = CleaningAction.Keep("Keep for RTL");
                    actions[CleaningPreset.SEOPlain] = CleaningAction.Replace(" ", "Replace with regular space");
                    break;

                case InvisibleCharacterCategory.ZeroWidthFormat:
                    switch (codePoint)
                    {
                        case 0x200C: // ZWNJ
                        case 0x200D: // ZWJ
                            actions[CleaningPreset.Safe] = CleaningAction.Keep("Keep ZWJ/ZWNJ");
                            actions[CleaningPreset.Aggressive] = CleaningAction.Remove("Remove ZWJ/ZWNJ");
                            actions[CleaningPreset.ASCIIStrict] = CleaningAction.Remove("Remove ZWJ/ZWNJ");
                            actions[CleaningPreset.TypographySoft] = CleaningAction.Keep("Keep ZWJ/ZWNJ");
                            actions[CleaningPreset.RTLSafe] = CleaningAction.Keep("Keep ZWJ/ZWNJ for RTL");
                            actions[CleaningPreset.SEOPlain] = CleaningAction.Remove("Remove ZWJ/ZWNJ");
                            break;
                        default:
                            actions[CleaningPreset.Safe] = CleaningAction.Remove("Remove zero-width char");
                            actions[CleaningPreset.Aggressive] = CleaningAction.Remove("Remove zero-width char");
                            actions[CleaningPreset.ASCIIStrict] = CleaningAction.Remove("Remove zero-width char");
                            actions[CleaningPreset.TypographySoft] = CleaningAction.Remove("Remove zero-width char");
                            actions[CleaningPreset.RTLSafe] = CleaningAction.Remove("Remove zero-width char");
                            actions[CleaningPreset.SEOPlain] = CleaningAction.Remove("Remove zero-width char");
                            break;
                    }
                    break;

                case InvisibleCharacterCategory.BiDiControls:
                    var isIsolate = codePoint >= 0x2066 && codePoint <= 0x2069; // LRI, RLI, FSI, PDI
                    
                    actions[CleaningPreset.Safe] = CleaningAction.Remove("Remove BiDi controls");
                    actions[CleaningPreset.Aggressive] = CleaningAction.Remove("Remove BiDi controls");
                    actions[CleaningPreset.ASCIIStrict] = CleaningAction.Remove("Remove BiDi controls");
                    actions[CleaningPreset.TypographySoft] = CleaningAction.Remove("Remove BiDi controls");
                    actions[CleaningPreset.RTLSafe] = isIsolate ? 
                        CleaningAction.Keep("Keep BiDi isolates for RTL") : 
                        CleaningAction.Remove("Remove obsolete BiDi controls");
                    actions[CleaningPreset.SEOPlain] = CleaningAction.Remove("Remove BiDi controls");
                    break;

                case InvisibleCharacterCategory.SoftHyphen:
                    actions[CleaningPreset.Safe] = CleaningAction.Remove("Remove soft hyphen");
                    actions[CleaningPreset.Aggressive] = CleaningAction.Remove("Remove soft hyphen");
                    actions[CleaningPreset.ASCIIStrict] = CleaningAction.Remove("Remove soft hyphen");
                    actions[CleaningPreset.TypographySoft] = CleaningAction.Keep("Keep soft hyphen for typography");
                    actions[CleaningPreset.RTLSafe] = CleaningAction.Remove("Remove soft hyphen");
                    actions[CleaningPreset.SEOPlain] = CleaningAction.Remove("Remove soft hyphen");
                    break;

                case InvisibleCharacterCategory.InvisibleMath:
                    actions[CleaningPreset.Safe] = CleaningAction.Remove("Remove invisible math");
                    actions[CleaningPreset.Aggressive] = CleaningAction.Remove("Remove invisible math");
                    actions[CleaningPreset.ASCIIStrict] = CleaningAction.Remove("Remove invisible math");
                    actions[CleaningPreset.TypographySoft] = CleaningAction.Conditional(ctx => ctx.Options.InvisibleMathToSpace ? " " : null, "Convert to space or remove");
                    actions[CleaningPreset.RTLSafe] = CleaningAction.Remove("Remove invisible math");
                    actions[CleaningPreset.SEOPlain] = CleaningAction.Remove("Remove invisible math");
                    break;

                case InvisibleCharacterCategory.VariationSelectors:
                    actions[CleaningPreset.Safe] = CleaningAction.Keep("Keep variation selectors");
                    actions[CleaningPreset.Aggressive] = CleaningAction.Remove("Remove variation selectors");
                    actions[CleaningPreset.ASCIIStrict] = CleaningAction.Remove("Remove variation selectors");
                    actions[CleaningPreset.TypographySoft] = CleaningAction.Keep("Keep variation selectors");
                    actions[CleaningPreset.RTLSafe] = CleaningAction.Keep("Keep variation selectors");
                    actions[CleaningPreset.SEOPlain] = CleaningAction.Remove("Remove variation selectors");
                    break;

                case InvisibleCharacterCategory.EmojiTags:
                    actions[CleaningPreset.Safe] = CleaningAction.Keep("Keep emoji tags");
                    actions[CleaningPreset.Aggressive] = CleaningAction.Remove("Remove emoji tags");
                    actions[CleaningPreset.ASCIIStrict] = CleaningAction.Remove("Remove emoji tags");
                    actions[CleaningPreset.TypographySoft] = CleaningAction.Keep("Keep emoji tags");
                    actions[CleaningPreset.RTLSafe] = CleaningAction.Keep("Keep emoji tags");
                    actions[CleaningPreset.SEOPlain] = CleaningAction.Remove("Remove emoji tags");
                    break;

                case InvisibleCharacterCategory.CombiningMarks:
                    actions[CleaningPreset.Safe] = CleaningAction.Conditional(ctx => HandleCombiningMark(ctx), "Keep with base or remove if orphaned");
                    actions[CleaningPreset.Aggressive] = CleaningAction.Remove("Remove combining marks");
                    actions[CleaningPreset.ASCIIStrict] = CleaningAction.Remove("Remove combining marks");
                    actions[CleaningPreset.TypographySoft] = CleaningAction.Keep("Keep combining marks");
                    actions[CleaningPreset.RTLSafe] = CleaningAction.Keep("Keep combining marks for RTL");
                    actions[CleaningPreset.SEOPlain] = CleaningAction.Remove("Remove combining marks");
                    break;

                case InvisibleCharacterCategory.Confusables:
                    var replacement = GetConfusableReplacement(codePoint);
                    actions[CleaningPreset.Safe] = CleaningAction.Keep("Keep confusables in safe mode");
                    actions[CleaningPreset.Aggressive] = CleaningAction.Replace(replacement, "Replace with ASCII equivalent");
                    actions[CleaningPreset.ASCIIStrict] = CleaningAction.Replace(replacement, "Replace with ASCII equivalent");
                    actions[CleaningPreset.TypographySoft] = CleaningAction.Keep("Keep for typography");
                    actions[CleaningPreset.RTLSafe] = CleaningAction.Keep("Keep confusables in RTL mode");
                    actions[CleaningPreset.SEOPlain] = CleaningAction.Replace(replacement, "Replace with ASCII equivalent");
                    break;

                default:
                    // Default fallback for unknown categories
                    actions[CleaningPreset.Safe] = CleaningAction.Keep("Keep unknown character");
                    actions[CleaningPreset.Aggressive] = CleaningAction.Remove("Remove unknown character");
                    actions[CleaningPreset.ASCIIStrict] = CleaningAction.Remove("Remove unknown character");
                    actions[CleaningPreset.TypographySoft] = CleaningAction.Keep("Keep unknown character");
                    actions[CleaningPreset.RTLSafe] = CleaningAction.Keep("Keep unknown character");
                    actions[CleaningPreset.SEOPlain] = CleaningAction.Remove("Remove unknown character");
                    break;
            }

            return actions;
        }

        private static string? HandleCR(CleaningContext ctx)
        {
            // If CR is followed by LF, remove the CR (it's part of CRLF sequence)
            if (ctx.NextCharacter?.Value == 0x000A) // LF
                return null; // Remove CR
            
            // Otherwise, replace CR with LF
            return "\n";
        }

        private static string? HandleCombiningMark(CleaningContext ctx)
        {
            // If there's a base character before this combining mark, keep it
            if (ctx.PreviousCharacter.HasValue && !IsInvisibleCharacter(ctx.PreviousCharacter.Value.Value))
                return ctx.CurrentCharacter.ToString(); // Keep the combining mark
            
            // Otherwise, it's orphaned, remove it
            return null;
        }

        private static bool IsInvisibleCharacter(int codePoint)
        {
            // This is a simplified check - in real implementation you'd use the full detection logic
            var category = CharUnicodeInfo.GetUnicodeCategory(codePoint);
            return category == UnicodeCategory.Format || 
                   category == UnicodeCategory.NonSpacingMark ||
                   category == UnicodeCategory.Control;
        }

        private static string GetConfusableReplacement(int codePoint)
        {
            // Confusables mapping
            return codePoint switch
            {
                0x201C => "\"", // LEFT DOUBLE QUOTATION MARK → ASCII
                0x201D => "\"", // RIGHT DOUBLE QUOTATION MARK → ASCII
                0x2018 => "'", // LEFT SINGLE QUOTATION MARK → ASCII
                0x2019 => "'", // RIGHT SINGLE QUOTATION MARK → ASCII
                0x2212 => "-", // MINUS SIGN → HYPHEN-MINUS
                0x2013 => "-", // EN DASH → HYPHEN-MINUS
                0x2014 => "-", // EM DASH → HYPHEN-MINUS
                0x0430 => "a", // CYRILLIC SMALL LETTER A → LATIN
                0x0435 => "e", // CYRILLIC SMALL LETTER IE → LATIN
                0x043E => "o", // CYRILLIC SMALL LETTER O → LATIN
                0x0440 => "p", // CYRILLIC SMALL LETTER ER → LATIN
                0x0441 => "c", // CYRILLIC SMALL LETTER ES → LATIN
                0x0443 => "y", // CYRILLIC SMALL LETTER U → LATIN
                0x0445 => "x", // CYRILLIC SMALL LETTER HA → LATIN
                _ => "?"        // Fallback
            };
        }
    }
}