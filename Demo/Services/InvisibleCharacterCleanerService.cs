using System.Globalization;
using System.Text;

namespace Demo.Services
{
    public class InvisibleCharacterCleanerService
    {
        private readonly InvisibleCharacterDetectorService _detector = new();

        // Character sets for cleaning (reuse from detector)
        private static readonly HashSet<int> C0C1Controls = new()
        {
            0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 
            0x0B, 0x0C, 0x0E, 0x0F, 0x10, 0x11, 0x12, 0x13, 0x14, 
            0x15, 0x16, 0x17, 0x18, 0x19, 0x1A, 0x1B, 0x1C, 0x1D, 
            0x1E, 0x1F, 0x7F, 0x80, 0x81, 0x82, 0x83, 0x84, 0x86, 
            0x87, 0x88, 0x89, 0x8A, 0x8B, 0x8C, 0x8D, 0x8E, 0x8F, 
            0x90, 0x91, 0x92, 0x93, 0x94, 0x95, 0x96, 0x97, 0x98, 
            0x99, 0x9A, 0x9B, 0x9C, 0x9D, 0x9E, 0x9F
        };

        private static readonly HashSet<int> WideSpaces = new()
        {
            0x2002, 0x2003, 0x2004, 0x2005, 0x2006, 0x2007, 
            0x2008, 0x2009, 0x200A, 0x205F, 0x3000
        };

        private static readonly HashSet<int> NoBreakSpaces = new() { 0x00A0, 0x202F };
        private static readonly HashSet<int> ZeroWidthFormat = new() { 0x200B, 0x200C, 0x200D, 0x2060, 0xFEFF, 0x180E };
        private static readonly HashSet<int> BiDiControls = new() { 0x200E, 0x200F, 0x202A, 0x202B, 0x202C, 0x202D, 0x202E, 0x2066, 0x2067, 0x2068, 0x2069 };
        private static readonly HashSet<int> InvisibleMath = new() { 0x2062, 0x2063, 0x2064 };
        private static readonly HashSet<int> VariationSelectors = new();
        private static readonly HashSet<int> EmojiTags = new();

        // BiDi isolates (keep in RTL-Safe mode)
        private static readonly HashSet<int> BiDiIsolates = new() { 0x2066, 0x2067, 0x2068, 0x2069 }; // LRI, RLI, FSI, PDI

        // Confusables mapping
        private static readonly Dictionary<int, string> ConfusablesAscii = new()
        {
            { 0x201C, "\"" }, { 0x201D, "\"" }, { 0x2018, "'" }, { 0x2019, "'" },
            { 0x2212, "-" }, { 0x2013, "-" }, { 0x2014, "-" },
            { 0x0430, "a" }, { 0x0435, "e" }, { 0x043E, "o" }, { 0x0440, "p" },
            { 0x0441, "c" }, { 0x0443, "y" }, { 0x0445, "x" }
        };

        // Typography normalization (for Typography-Soft preset)
        private static readonly Dictionary<int, string> TypographyNormalization = new()
        {
            { 0x2212, "−" }, // Keep MINUS in math contexts
            { 0x2013, "–" }, // Keep EN DASH for ranges
            { 0x2014, "—" }, // Keep EM DASH for dialogs
            { 0x201C, """ }, { 0x201D, """ }, // Smart quotes
            { 0x2018, "'" }, { 0x2019, "'" }
        };

        static InvisibleCharacterCleanerService()
        {
            // Initialize variation selectors and emoji tags
            for (int i = 0xFE00; i <= 0xFE0F; i++) VariationSelectors.Add(i);
            for (int i = 0xE0100; i <= 0xE01EF; i++) VariationSelectors.Add(i);
            EmojiTags.Add(0xE0001);
            for (int i = 0xE0020; i <= 0xE007F; i++) EmojiTags.Add(i);
        }

        public CleaningResult CleanText(string input, CleaningPreset preset, CleaningOptions? options = null)
        {
            options ??= GetDefaultOptions(preset);
            
            var detectionResult = _detector.DetectInvisibleCharacters(input, options.SkipCodeBlocks);
            var cleanedText = ApplyCleaningRules(input, preset, options);
            
            var afterCleaningResult = _detector.DetectInvisibleCharacters(cleanedText, options.SkipCodeBlocks);
            
            return new CleaningResult
            {
                OriginalText = input,
                CleanedText = cleanedText,
                OriginalDetection = detectionResult,
                AfterCleaningDetection = afterCleaningResult,
                Preset = preset,
                Options = options,
                Statistics = CalculateStatistics(detectionResult, afterCleaningResult)
            };
        }

        private string ApplyCleaningRules(string input, CleaningPreset preset, CleaningOptions options)
        {
            var sb = new StringBuilder(input.Length);
            var codeBlockRanges = options.SkipCodeBlocks ? FindCodeBlockRanges(input) : new List<(int start, int end)>();
            
            int position = 0;
            var runes = input.EnumerateRunes().ToArray();
            bool insertedSpaceForInvisibleSequence = false;
            
            for (int i = 0; i < runes.Length; i++)
            {
                var rune = runes[i];
                var previousRune = i > 0 ? (Rune?)runes[i - 1] : null;
                var nextRune = i < runes.Length - 1 ? (Rune?)runes[i + 1] : null;

                // Skip processing if in code block
                if (options.SkipCodeBlocks && IsInCodeBlock(position, codeBlockRanges))
                {
                    sb.Append(rune.ToString());
                    position += rune.Utf16SequenceLength;
                    continue;
                }

                // Check if this character is invisible and would cause word merging
                bool isInvisible = IsInvisibleCharacter(rune.Value);
                // Temporarily disable word boundary checks
                // bool shouldCheckWordBoundary = ShouldCheckWordBoundary(rune.Value);
                
                // if (isInvisible && shouldCheckWordBoundary)
                // {
                //     // If this is the first invisible character in a sequence and would merge words
                //     if (!insertedSpaceForInvisibleSequence && WouldMergeWords(runes, i))
                //     {
                //         sb.Append(' ');
                //         insertedSpaceForInvisibleSequence = true;
                //     }
                // }
                // else 
                if (!isInvisible)
                {
                    // Reset flag when we encounter a visible character
                    insertedSpaceForInvisibleSequence = false;
                }

                var processed = ProcessCharacter(rune, preset, options, previousRune, nextRune, sb, runes, i);
                if (processed != null)
                {
                    sb.Append(processed);
                }

                position += rune.Utf16SequenceLength;
            }

            return sb.ToString();
        }

        private string? ProcessCharacter(Rune rune, CleaningPreset preset, CleaningOptions options, Rune? previousRune, Rune? nextRune, StringBuilder sb, Rune[] runes, int index)
        {
            var codePoint = rune.Value;
            var category = CharUnicodeInfo.GetUnicodeCategory(codePoint);

            // Category 1: C0/C1 Controls (except \t \n \r)
            if (C0C1Controls.Contains(codePoint))
                return null; // Remove

            // Category 2: Line breaks - normalize
            // Handle CRLF as a unit - skip LF if previous was CR
            if (codePoint == 0x000A && previousRune?.Value == 0x000D) // LF after CR
                return null; // Skip the LF in CRLF

            if (codePoint is 0x000D or 0x0085 or 0x2028 or 0x2029)
                return "\n";

            if (codePoint == 0x000A) // Standalone LF
                return "\n";

            // Category 3: TAB
            if (codePoint == 0x0009)
                return new string(' ', options.TabSize);

            // Category 4: Wide spaces
            if (WideSpaces.Contains(codePoint))
                return " ";

            // Category 5: Non-breaking spaces
            if (NoBreakSpaces.Contains(codePoint))
            {
                return preset switch
                {
                    CleaningPreset.Safe or CleaningPreset.TypographySoft or CleaningPreset.RTLSafe => rune.ToString(), // Keep
                    _ => " " // Convert to regular space
                };
            }

            // Category 6: Zero-width/format
            if (ZeroWidthFormat.Contains(codePoint))
            {
                // Special handling for ZWJ/ZWNJ in some presets
                if ((codePoint == 0x200C || codePoint == 0x200D) && options.PreserveZWJZWNJ)
                    return rune.ToString();
                
                // ZWSP between letters becomes a space only in word-separated contexts
                if (codePoint == 0x200B && IsLetterCharacter(previousRune) && HasLetterAhead(runes, index + 1))
                {
                    // Only add space if there are already spaces in the surrounding context
                    // This distinguishes between word separation (should add space) and 
                    // compound words/formatting (should not add space)
                    if (HasSpaceInSurroundingContext(sb, runes, index))
                        return " ";
                }
                
                return null; // Remove
            }

            // Category 7: BiDi controls
            if (BiDiControls.Contains(codePoint))
            {
                // RTL-Safe: keep isolates, remove embeddings
                if (preset == CleaningPreset.RTLSafe && BiDiIsolates.Contains(codePoint))
                    return rune.ToString();
                
                return null; // Remove
            }

            // Category 8: Soft hyphen
            if (codePoint == 0x00AD)
                return null; // Remove

            // Category 9: Invisible math  
            if (InvisibleMath.Contains(codePoint))
                return options.InvisibleMathToSpace ? " " : null;

            // Category 10: Variation selectors
            if (VariationSelectors.Contains(codePoint))
            {
                return preset switch
                {
                    CleaningPreset.Safe or CleaningPreset.TypographySoft or CleaningPreset.RTLSafe => rune.ToString(), // Keep
                    _ => null // Remove
                };
            }

            // Category 11: Emoji tags
            if (EmojiTags.Contains(codePoint))
            {
                return preset switch
                {
                    CleaningPreset.Safe or CleaningPreset.TypographySoft or CleaningPreset.RTLSafe => rune.ToString(), // Keep
                    _ => null // Remove
                };
            }

            // Category 12: Combining marks - remove if orphaned
            if (category == UnicodeCategory.NonSpacingMark)
            {
                bool hasBase = HasValidBase(previousRune);
                if (!hasBase)
                    return null; // Remove orphaned combining marks
                return rune.ToString(); // Keep with base
            }

            // Category 13: Confusables
            if (ConfusablesAscii.ContainsKey(codePoint))
            {
                return preset switch
                {
                    CleaningPreset.TypographySoft => GetTypographyNormalization(codePoint),
                    CleaningPreset.ASCIIStrict or CleaningPreset.Aggressive or CleaningPreset.SEOPlain => ConfusablesAscii[codePoint],
                    _ => rune.ToString() // Keep original in Safe and RTLSafe
                };
            }

            return rune.ToString(); // Keep character as is
        }

        private string GetTypographyNormalization(int codePoint)
        {
            // For typography-soft, only convert certain confusables, keep others as typography
            return codePoint switch
            {
                0x201C => "\u201C", // LEFT DOUBLE QUOTATION MARK - keep as smart quote
                0x201D => "\u201D", // RIGHT DOUBLE QUOTATION MARK - keep as smart quote  
                0x2018 => "\u2018", // LEFT SINGLE QUOTATION MARK - keep as smart quote
                0x2019 => "\u2019", // RIGHT SINGLE QUOTATION MARK - keep as smart quote
                0x2013 => "\u2013", // EN DASH - keep as en dash
                0x2014 => "\u2014", // EM DASH - keep as em dash
                0x2212 => "\u2212", // MINUS SIGN - keep as math minus
                // Cyrillic lookalikes should be converted to Latin
                0x0430 => "a", // CYRILLIC SMALL LETTER A → LATIN
                0x0435 => "e", // CYRILLIC SMALL LETTER IE → LATIN  
                0x043E => "o", // CYRILLIC SMALL LETTER O → LATIN
                0x0440 => "p", // CYRILLIC SMALL LETTER ER → LATIN
                0x0441 => "c", // CYRILLIC SMALL LETTER ES → LATIN
                0x0443 => "y", // CYRILLIC SMALL LETTER U → LATIN
                0x0445 => "x", // CYRILLIC SMALL LETTER HA → LATIN
                _ => ConfusablesAscii.TryGetValue(codePoint, out var ascii) ? ascii : ((char)codePoint).ToString()
            };
        }

        private bool HasValidBase(Rune? previousRune)
        {
            if (!previousRune.HasValue)
                return false;

            var category = CharUnicodeInfo.GetUnicodeCategory(previousRune.Value.Value);
            return category is UnicodeCategory.LowercaseLetter or 
                              UnicodeCategory.UppercaseLetter or 
                              UnicodeCategory.TitlecaseLetter or 
                              UnicodeCategory.ModifierLetter or 
                              UnicodeCategory.OtherLetter or 
                              UnicodeCategory.DecimalDigitNumber or 
                              UnicodeCategory.LetterNumber or 
                              UnicodeCategory.OtherNumber or 
                              UnicodeCategory.NonSpacingMark or 
                              UnicodeCategory.SpacingCombiningMark or 
                              UnicodeCategory.EnclosingMark;
        }

        private bool ShouldInsertSpaceForWordBoundary(Rune? previousRune, Rune? nextRune)
        {
            // Insert space if we have letters on both sides to prevent word merging
            if (previousRune == null || nextRune == null)
                return false;

            var prevCategory = CharUnicodeInfo.GetUnicodeCategory(previousRune.Value.Value);
            bool isPrevLetter = IsLetter(prevCategory);

            // Look ahead past consecutive invisible characters to find the next visible character
            var nextVisibleRune = nextRune;
            while (nextVisibleRune != null && IsInvisibleCharacter(nextVisibleRune.Value.Value))
            {
                // This is a simplified check - in a real implementation, we'd need to look further ahead
                // For now, just check the immediate next rune
                break;
            }

            if (nextVisibleRune == null)
                return false;

            var nextCategory = CharUnicodeInfo.GetUnicodeCategory(nextVisibleRune.Value.Value);
            bool isNextLetter = IsLetter(nextCategory);

            return isPrevLetter && isNextLetter;
        }

        private bool IsLetter(UnicodeCategory category)
        {
            return category is UnicodeCategory.LowercaseLetter or 
                              UnicodeCategory.UppercaseLetter or 
                              UnicodeCategory.TitlecaseLetter or 
                              UnicodeCategory.ModifierLetter or 
                              UnicodeCategory.OtherLetter or
                              UnicodeCategory.DecimalDigitNumber;
        }

        private bool IsLetterCharacter(Rune? rune)
        {
            if (!rune.HasValue) return false;
            var category = CharUnicodeInfo.GetUnicodeCategory(rune.Value.Value);
            return IsLetter(category);
        }

        private bool HasSpaceInSurroundingContext(StringBuilder sb, Rune[] runes, int index)
        {
            // Check if there are spaces in the already processed text (StringBuilder)
            var processed = sb.ToString();
            if (processed.Length > 0 && (processed[processed.Length - 1] == ' ' || processed.Contains(' ')))
                return true;

            // Check if there are spaces in the upcoming text (next few characters)
            for (int i = index + 1; i < Math.Min(index + 10, runes.Length); i++)
            {
                if (runes[i].Value == 0x0020 || runes[i].Value == 0x00A0) // space or NBSP
                    return true;
                if (!IsInvisibleCharacter(runes[i].Value))
                    break; // Stop at first visible character
            }
            
            return false;
        }

        private bool HasLetterAhead(Rune[] runes, int startIndex)
        {
            for (int i = startIndex; i < runes.Length; i++)
            {
                if (IsLetterCharacter(runes[i]))
                    return true;
                // If we encounter a visible character that's not a letter, stop looking
                if (!IsInvisibleCharacter(runes[i].Value))
                    return false;
            }
            return false;
        }

        private bool ShouldCheckWordBoundary(int codePoint)
        {
            // Only zero-width characters and BiDi controls should trigger word boundary checks
            // Soft hyphens, invisible math, etc. should NOT create word boundaries
            return ZeroWidthFormat.Contains(codePoint) || BiDiControls.Contains(codePoint);
        }

        private bool IsInvisibleCharacter(int codePoint)
        {
            return C0C1Controls.Contains(codePoint) ||
                   ZeroWidthFormat.Contains(codePoint) ||
                   BiDiControls.Contains(codePoint) ||
                   codePoint == 0x00AD || // Soft hyphen
                   InvisibleMath.Contains(codePoint) ||
                   VariationSelectors.Contains(codePoint) ||
                   EmojiTags.Contains(codePoint);
        }

        private bool WouldMergeWords(Rune[] runes, int startInvisibleIndex)
        {
            // Find the last visible character before the invisible sequence
            int prevVisibleIndex = startInvisibleIndex - 1;
            while (prevVisibleIndex >= 0 && IsInvisibleCharacter(runes[prevVisibleIndex].Value))
                prevVisibleIndex--;

            // Find the first visible character after the invisible sequence
            int nextVisibleIndex = startInvisibleIndex + 1;
            while (nextVisibleIndex < runes.Length && IsInvisibleCharacter(runes[nextVisibleIndex].Value))
                nextVisibleIndex++;

            if (prevVisibleIndex < 0 || nextVisibleIndex >= runes.Length)
                return false;

            var prevCategory = CharUnicodeInfo.GetUnicodeCategory(runes[prevVisibleIndex].Value);
            var nextCategory = CharUnicodeInfo.GetUnicodeCategory(runes[nextVisibleIndex].Value);

            return IsLetter(prevCategory) && IsLetter(nextCategory);
        }

        private List<(int start, int end)> FindCodeBlockRanges(string input)
        {
            var ranges = new List<(int start, int end)>();
            bool inInlineCode = false;
            bool inFencedCode = false;
            int codeStart = 0;
            
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == '`')
                {
                    if (i + 2 < input.Length && input[i + 1] == '`' && input[i + 2] == '`')
                    {
                        if (!inFencedCode)
                        {
                            inFencedCode = true;
                            codeStart = i;
                        }
                        else
                        {
                            inFencedCode = false;
                            ranges.Add((codeStart, i + 3));
                        }
                        i += 2;
                    }
                    else if (!inFencedCode)
                    {
                        if (!inInlineCode)
                        {
                            inInlineCode = true;
                            codeStart = i;
                        }
                        else
                        {
                            inInlineCode = false;
                            ranges.Add((codeStart, i + 1));
                        }
                    }
                }
            }
            
            return ranges;
        }

        private bool IsInCodeBlock(int position, List<(int start, int end)> codeBlocks)
        {
            return codeBlocks.Any(block => position >= block.start && position < block.end);
        }

        private CleaningStatistics CalculateStatistics(DetectionResult original, DetectionResult afterCleaning)
        {
            var stats = new CleaningStatistics();
            
            foreach (var category in Enum.GetValues<InvisibleCharacterCategory>())
            {
                int originalCount = original.CategoryCounts.GetValueOrDefault(category, 0);
                int afterCount = afterCleaning.CategoryCounts.GetValueOrDefault(category, 0);
                int removed = originalCount - afterCount;
                
                if (removed > 0)
                {
                    stats.RemovedCounts[category] = removed;
                    stats.TotalRemoved += removed;
                }
            }

            return stats;
        }

        public static CleaningOptions GetDefaultOptions(CleaningPreset preset)
        {
            return preset switch
            {
                CleaningPreset.Safe => new CleaningOptions
                {
                    SkipCodeBlocks = true,
                    TabSize = 4,
                    PreserveZWJZWNJ = false,
                    InvisibleMathToSpace = true
                },
                CleaningPreset.Aggressive => new CleaningOptions
                {
                    SkipCodeBlocks = true,
                    TabSize = 4,
                    PreserveZWJZWNJ = false,
                    InvisibleMathToSpace = true
                },
                CleaningPreset.ASCIIStrict => new CleaningOptions
                {
                    SkipCodeBlocks = true,
                    TabSize = 4,
                    PreserveZWJZWNJ = false,
                    InvisibleMathToSpace = false
                },
                CleaningPreset.TypographySoft => new CleaningOptions
                {
                    SkipCodeBlocks = true,
                    TabSize = 4,
                    PreserveZWJZWNJ = true,
                    InvisibleMathToSpace = true
                },
                CleaningPreset.RTLSafe => new CleaningOptions
                {
                    SkipCodeBlocks = true,
                    TabSize = 4,
                    PreserveZWJZWNJ = true,
                    InvisibleMathToSpace = true
                },
                CleaningPreset.SEOPlain => new CleaningOptions
                {
                    SkipCodeBlocks = true,
                    TabSize = 4,
                    PreserveZWJZWNJ = false,
                    InvisibleMathToSpace = false
                },
                _ => throw new ArgumentOutOfRangeException(nameof(preset))
            };
        }
    }

    public class CleaningResult
    {
        public string OriginalText { get; set; } = string.Empty;
        public string CleanedText { get; set; } = string.Empty;
        public DetectionResult OriginalDetection { get; set; } = new();
        public DetectionResult AfterCleaningDetection { get; set; } = new();
        public CleaningPreset Preset { get; set; }
        public CleaningOptions Options { get; set; } = new();
        public CleaningStatistics Statistics { get; set; } = new();
        
        public bool HasChanges => OriginalText != CleanedText;
        public string Summary => GenerateSummary();
        
        private string GenerateSummary()
        {
            if (!HasChanges)
                return "No changes made.";

            var parts = new List<string>();
            
            foreach (var (category, count) in Statistics.RemovedCounts)
            {
                var categoryName = category switch
                {
                    InvisibleCharacterCategory.C0C1Controls => "control characters",
                    InvisibleCharacterCategory.LineBreaks => "line breaks normalized",
                    InvisibleCharacterCategory.Tab => "tabs converted",
                    InvisibleCharacterCategory.WideSpaces => "wide spaces",
                    InvisibleCharacterCategory.NoBreakSpaces => "non-breaking spaces",
                    InvisibleCharacterCategory.ZeroWidthFormat => "zero-width characters",
                    InvisibleCharacterCategory.BiDiControls => "BiDi controls",
                    InvisibleCharacterCategory.SoftHyphen => "soft hyphens",
                    InvisibleCharacterCategory.InvisibleMath => "invisible math operators",
                    InvisibleCharacterCategory.VariationSelectors => "variation selectors",
                    InvisibleCharacterCategory.EmojiTags => "emoji tags",
                    InvisibleCharacterCategory.CombiningMarks => "orphaned combining marks",
                    InvisibleCharacterCategory.Confusables => "confusable characters",
                    _ => "characters"
                };
                
                parts.Add($"{count} {categoryName}");
            }
            
            return $"Processed: {string.Join(", ", parts)}. Total: {Statistics.TotalRemoved} characters affected.";
        }
    }

    public class CleaningOptions
    {
        public bool SkipCodeBlocks { get; set; } = true;
        public int TabSize { get; set; } = 4;
        public bool PreserveZWJZWNJ { get; set; } = false;
        public bool InvisibleMathToSpace { get; set; } = true;
        public HashSet<int> WhitelistedCharacters { get; set; } = new();
        public HashSet<int> BlacklistedCharacters { get; set; } = new();
    }

    public class CleaningStatistics
    {
        public Dictionary<InvisibleCharacterCategory, int> RemovedCounts { get; set; } = new();
        public int TotalRemoved { get; set; } = 0;
    }

    public enum CleaningPreset
    {
        Safe,           // Default universal cleaning
        Aggressive,     // Maximum cleaning
        ASCIIStrict,    // Code/documentation, CI builds  
        TypographySoft, // Publications/typesetting
        RTLSafe,        // Texts with Arabic/Hebrew
        SEOPlain        // Blogs/content transfer
    }
}