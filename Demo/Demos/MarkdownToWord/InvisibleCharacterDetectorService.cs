using System.Buffers;
using System.Globalization;
using System.Text;

namespace Demo.Demos.MarkdownToWord
{
    public class InvisibleCharacterDetectorService
    {
        // Character sets for different categories
        private static readonly HashSet<int> C0C1Controls = new();

        static InvisibleCharacterDetectorService()
        {
            // Initialize C0/C1 controls (excluding NEL which is handled as line break)
            for (var i = 0x00; i <= 0x1F; i++)
            {
                if (i != 0x09 && i != 0x0A && i != 0x0D) // Skip TAB, LF, CR
                    C0C1Controls.Add(i);
            }
            C0C1Controls.Add(0x7F); // DEL
            for (var i = 0x80; i <= 0x9F; i++)
            {
                if (i != 0x85) // Skip NEL (handled as line break)
                    C0C1Controls.Add(i);
            }

            // Initialize wide spaces - comprehensive list
            var wideSpacesList = new[]
            {
                0x2000, // EN QUAD
                0x2001, // EM QUAD
                0x2002, // EN SPACE
                0x2003, // EM SPACE
                0x2004, // THREE-PER-EM SPACE
                0x2005, // FOUR-PER-EM SPACE
                0x2006, // SIX-PER-EM SPACE
                0x2007, // FIGURE SPACE
                0x2008, // PUNCTUATION SPACE
                0x2009, // THIN SPACE
                0x200A, // HAIR SPACE
                0x205F, // MEDIUM MATHEMATICAL SPACE
                0x3000  // IDEOGRAPHIC SPACE
            };
            foreach (var space in wideSpacesList)
                WideSpaces.Add(space);

            // Initialize no-break spaces
            NoBreakSpaces.Add(0x00A0); // NBSP
            NoBreakSpaces.Add(0x202F); // NARROW NO-BREAK SPACE

            // Initialize zero-width formatting characters
            var zeroWidthList = new[]
            {
                0x200B, // ZWSP
                0x200C, // ZWNJ
                0x200D, // ZWJ
                0x2060, // WORD JOINER
                0xFEFF, // BOM/ZWNBSP
                0x180E  // MONGOLIAN VOWEL SEPARATOR (historical)
            };
            foreach (var zw in zeroWidthList)
                ZeroWidthFormat.Add(zw);

            // Initialize BiDi controls
            var biDiList = new[]
            {
                0x200E, // LRM
                0x200F, // RLM
                0x202A, // LRE
                0x202B, // RLE
                0x202C, // PDF
                0x202D, // LRO
                0x202E, // RLO
                0x2066, // LRI
                0x2067, // RLI
                0x2068, // FSI
                0x2069  // PDI
            };
            foreach (var bidi in biDiList)
                BiDiControls.Add(bidi);

            // Initialize invisible math operators
            InvisibleMath.Add(0x2062); // INVISIBLE TIMES
            InvisibleMath.Add(0x2063); // INVISIBLE SEPARATOR  
            InvisibleMath.Add(0x2064); // INVISIBLE PLUS

            // Initialize variation selectors
            for (var i = 0xFE00; i <= 0xFE0F; i++)
                VariationSelectors.Add(i);
            for (var i = 0xE0100; i <= 0xE01EF; i++)
                VariationSelectors.Add(i);

            // Initialize emoji tags - full TAG block
            for (var i = 0xE0000; i <= 0xE007F; i++)
                EmojiTags.Add(i);
        }

        private static readonly HashSet<int> LineBreaks = new()
        {
            // Note: Regular LF (0x000A) is not included as it's a normal text formatting character
            // Only exotic line breaks are considered "invisible characters"
            0x000D, // CR
            0x0085, // NEL
            0x2028, // LS
            0x2029  // PS
        };

        private static readonly HashSet<int> WideSpaces = new();
        private static readonly HashSet<int> NoBreakSpaces = new();
        private static readonly HashSet<int> ZeroWidthFormat = new();
        private static readonly HashSet<int> BiDiControls = new();
        private static readonly HashSet<int> InvisibleMath = new();
        private static readonly HashSet<int> VariationSelectors = new();
        private static readonly HashSet<int> EmojiTags = new();

        public DetectionResult DetectInvisibleCharacters(string input, bool skipCodeBlocks = true)
        {
            var result = new DetectionResult();
            var codeBlockRanges = skipCodeBlocks ? FindCodeBlockRanges(input) : new List<(int start, int end)>();
            var scriptProfile = AnalyzeScriptProfile(input);

            var utf16Position = 0;
            var line = 1;
            var column = 1;

            foreach (var rune in input.EnumerateRunes())
            {
                // Skip if in code block
                if (skipCodeBlocks && IsInCodeBlock(utf16Position, codeBlockRanges))
                {
                    UpdatePosition(rune, ref line, ref column, ref utf16Position);
                    continue;
                }

                var detection = ClassifyCharacter(input, rune, utf16Position, scriptProfile);
                if (detection != null)
                {
                    detection.Position = utf16Position;
                    detection.Line = line;
                    detection.Column = column;
                    detection.Context = GetContext(input, utf16Position, 10);
                    
                    result.DetectedCharacters.Add(detection);
                    result.CategoryCounts[detection.Category] = 
                        result.CategoryCounts.GetValueOrDefault(detection.Category, 0) + 1;
                }

                UpdatePosition(rune, ref line, ref column, ref utf16Position);
            }

            return result;
        }

        private CharacterDetection? ClassifyCharacter(string text, Rune rune, int position, ScriptProfile scriptProfile)
        {
            var codePoint = rune.Value;
            var category = CharUnicodeInfo.GetUnicodeCategory(codePoint);

            // Category 1: ASCII Control C0/C1
            if (C0C1Controls.Contains(codePoint))
            {
                return CreateCharacterDetection(InvisibleCharacterCategory.C0C1Controls, codePoint, "CTRL");
            }

            // Category 2: Line breaks
            if (LineBreaks.Contains(codePoint))
            {
                return CreateCharacterDetection(InvisibleCharacterCategory.LineBreaks, codePoint, "¶");
            }

            // Category 3: Tab
            if (codePoint == 0x0009)
            {
                return CreateCharacterDetection(InvisibleCharacterCategory.Tab, codePoint, "→");
            }

            // Category 4: Wide spaces
            if (WideSpaces.Contains(codePoint))
            {
                return CreateCharacterDetection(InvisibleCharacterCategory.WideSpaces, codePoint, "·");
            }

            // Category 5: Non-breaking spaces
            if (NoBreakSpaces.Contains(codePoint))
            {
                return CreateCharacterDetection(InvisibleCharacterCategory.NoBreakSpaces, codePoint, codePoint == 0x00A0 ? "⍽" : "⍽ⁿ");
            }

            // Category 6: Zero-width/format
            if (ZeroWidthFormat.Contains(codePoint))
            {
                return CreateCharacterDetection(InvisibleCharacterCategory.ZeroWidthFormat, codePoint, GetZeroWidthMarker(codePoint));
            }

            // Category 7: BiDi controls
            if (BiDiControls.Contains(codePoint))
            {
                return CreateCharacterDetection(InvisibleCharacterCategory.BiDiControls, codePoint, GetBiDiMarker(codePoint));
            }

            // Category 8: Soft hyphen
            if (codePoint == 0x00AD)
            {
                return CreateCharacterDetection(InvisibleCharacterCategory.SoftHyphen, codePoint, "¬");
            }

            // Category 9: Invisible math
            if (InvisibleMath.Contains(codePoint))
            {
                return CreateCharacterDetection(InvisibleCharacterCategory.InvisibleMath, codePoint, GetMathMarker(codePoint));
            }

            // Category 10: Variation selectors
            if (VariationSelectors.Contains(codePoint))
            {
                return CreateCharacterDetection(InvisibleCharacterCategory.VariationSelectors, codePoint, $"VS{(codePoint >= 0xFE00 && codePoint <= 0xFE0F ? codePoint - 0xFE00 + 1 : "S")}");
            }

            // Category 11: Emoji tags
            if (EmojiTags.Contains(codePoint))
            {
                return CreateCharacterDetection(InvisibleCharacterCategory.EmojiTags, codePoint, "TAG");
            }

            // Category 12: Combining marks
            if (category == UnicodeCategory.NonSpacingMark)
            {
                return CreateCharacterDetection(InvisibleCharacterCategory.CombiningMarks, codePoint, "◌");
            }

            // Category 13: Confusables
            if (ConfusableCharacterDefinitions.Punctuation.ContainsKey(codePoint))
            {
                return CreateCharacterDetection(InvisibleCharacterCategory.Confusables, codePoint, "≈");
            }

            if (ConfusableCharacterDefinitions.Letters.TryGetValue(codePoint, out var letterDefinition))
            {
                if (IsSuspiciousContext(letterDefinition, text, position, rune, scriptProfile))
                {
                    return CreateCharacterDetection(InvisibleCharacterCategory.Confusables, codePoint, "≈");
                }

                return null;
            }

            return null;
        }

        private static ScriptProfile AnalyzeScriptProfile(string text)
        {
            var latinCount = 0;
            var cyrillicCount = 0;

            foreach (var rune in text.EnumerateRunes())
            {
                if (IsLatinLetter(rune))
                {
                    latinCount++;
                }

                if (IsCyrillicLetter(rune))
                {
                    cyrillicCount++;
                }
            }

            return new ScriptProfile(latinCount, cyrillicCount);
        }

        private static bool IsSuspiciousContext(
            ConfusableLetterDefinition definition,
            string text,
            int position,
            Rune currentRune,
            ScriptProfile scriptProfile)
        {
            return definition.RequiredContext switch
            {
                ConfusableLetterContext.Latin => IsSuspiciousLatinContext(text, position, currentRune, scriptProfile),
                ConfusableLetterContext.Cyrillic => IsSuspiciousCyrillicContext(text, position, currentRune, scriptProfile),
                _ => true
            };
        }

        private static bool IsSuspiciousLatinContext(string text, int position, Rune currentRune, ScriptProfile scriptProfile)
        {
            var wordProfile = GetWordScriptProfile(text, position, currentRune);

            if (!wordProfile.ContainsLatinLetters)
            {
                if (wordProfile.CyrillicLetterCount > 1)
                {
                    return false;
                }

                if (scriptProfile.CyrillicLetterCount > wordProfile.CyrillicLetterCount)
                {
                    return false;
                }

                return scriptProfile.LatinLetterCount > 0;
            }

            return true;
        }

        private static bool IsSuspiciousCyrillicContext(string text, int position, Rune currentRune, ScriptProfile scriptProfile)
        {
            var wordProfile = GetWordScriptProfile(text, position, currentRune);

            if (!wordProfile.ContainsCyrillicLetters)
            {
                if (wordProfile.LatinLetterCount > 1)
                {
                    return false;
                }

                if (scriptProfile.LatinLetterCount > wordProfile.LatinLetterCount)
                {
                    return false;
                }

                return scriptProfile.CyrillicLetterCount > 0;
            }

            return true;
        }

        private static WordScriptProfile GetWordScriptProfile(string text, int position, Rune currentRune)
        {
            var latinCount = IsLatinLetter(currentRune) ? 1 : 0;
            var cyrillicCount = IsCyrillicLetter(currentRune) ? 1 : 0;
            var otherCount = IsLetterCategory(CharUnicodeInfo.GetUnicodeCategory(currentRune.Value)) &&
                             !IsLatinLetter(currentRune) &&
                             !IsCyrillicLetter(currentRune)
                ? 1
                : 0;

            var index = position;

            while (true)
            {
                index = GetPreviousRuneStartIndex(text, index);
                if (index < 0)
                {
                    break;
                }

                if (!TryGetRuneAt(text, index, out var rune, out _))
                {
                    break;
                }

                if (!IsLetterCategory(CharUnicodeInfo.GetUnicodeCategory(rune.Value)))
                {
                    break;
                }

                if (IsLatinLetter(rune))
                {
                    latinCount++;
                }
                else if (IsCyrillicLetter(rune))
                {
                    cyrillicCount++;
                }
                else
                {
                    otherCount++;
                }
            }

            index = position + currentRune.Utf16SequenceLength;

            while (index < text.Length)
            {
                if (!TryGetRuneAt(text, index, out var rune, out var consumed))
                {
                    break;
                }

                if (!IsLetterCategory(CharUnicodeInfo.GetUnicodeCategory(rune.Value)))
                {
                    break;
                }

                if (IsLatinLetter(rune))
                {
                    latinCount++;
                }
                else if (IsCyrillicLetter(rune))
                {
                    cyrillicCount++;
                }
                else
                {
                    otherCount++;
                }

                index += consumed;
            }

            return new WordScriptProfile(latinCount, cyrillicCount, otherCount);
        }

        private static int GetPreviousRuneStartIndex(string text, int position)
        {
            var index = position - 1;

            while (index >= 0 && char.IsLowSurrogate(text[index]))
            {
                index--;
            }

            return index;
        }

        private static bool TryGetRuneAt(string text, int index, out Rune rune, out int length)
        {
            if (index < 0 || index >= text.Length)
            {
                rune = default;
                length = 0;
                return false;
            }

            var status = Rune.DecodeFromUtf16(text.AsSpan(index), out rune, out length);
            return status == OperationStatus.Done;
        }

        private static bool IsLetterCategory(UnicodeCategory category) =>
            category == UnicodeCategory.UppercaseLetter ||
            category == UnicodeCategory.LowercaseLetter ||
            category == UnicodeCategory.TitlecaseLetter ||
            category == UnicodeCategory.OtherLetter ||
            category == UnicodeCategory.ModifierLetter;

        private static bool IsLatinLetter(Rune rune)
        {
            var value = rune.Value;

            if ((value >= 0x0041 && value <= 0x005A) || (value >= 0x0061 && value <= 0x007A))
            {
                return true;
            }

            if ((value >= 0x00C0 && value <= 0x00D6) || (value >= 0x00D8 && value <= 0x00F6) || (value >= 0x00F8 && value <= 0x00FF))
            {
                return true;
            }

            if (value >= 0x0100 && value <= 0x017F)
            {
                return true;
            }

            if (value >= 0x0180 && value <= 0x024F)
            {
                return true;
            }

            return false;
        }

        private static bool IsCyrillicLetter(Rune rune)
        {
            var value = rune.Value;

            if (value >= 0x0400 && value <= 0x04FF)
            {
                return true;
            }

            if (value >= 0x0500 && value <= 0x052F)
            {
                return true;
            }

            if (value >= 0x2DE0 && value <= 0x2DFF)
            {
                return true;
            }

            if (value >= 0xA640 && value <= 0xA69F)
            {
                return true;
            }

            if (value >= 0x1C80 && value <= 0x1C8F)
            {
                return true;
            }

            return false;
        }

        private readonly struct WordScriptProfile
        {
            public WordScriptProfile(int latinLetterCount, int cyrillicLetterCount, int otherLetterCount)
            {
                LatinLetterCount = latinLetterCount;
                CyrillicLetterCount = cyrillicLetterCount;
                OtherLetterCount = otherLetterCount;
            }

            public int LatinLetterCount { get; }
            public int CyrillicLetterCount { get; }
            public int OtherLetterCount { get; }

            public bool ContainsLatinLetters => LatinLetterCount > 0;
            public bool ContainsCyrillicLetters => CyrillicLetterCount > 0;
        }

        private readonly struct ScriptProfile
        {
            public ScriptProfile(int latinLetterCount, int cyrillicLetterCount)
            {
                LatinLetterCount = latinLetterCount;
                CyrillicLetterCount = cyrillicLetterCount;
            }

            public int LatinLetterCount { get; }
            public int CyrillicLetterCount { get; }
        }

        private static List<(int start, int end)> FindCodeBlockRanges(string input)
        {
            var ranges = new List<(int start, int end)>();
            var inInlineCode = false;
            var inFencedCode = false;
            var codeStart = 0;
            
            for (var i = 0; i < input.Length; i++)
            {
                if (input[i] == '`')
                {
                    // Check for fenced code blocks (```)
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
                        i += 2; // Skip the next two backticks
                    }
                    // Inline code
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
                else if (input[i] == '\n' && inFencedCode)
                {
                    // Don't break out of fenced code blocks on newlines
                    continue;
                }
            }
            
            return ranges;
        }

        private static bool IsInCodeBlock(int position, List<(int start, int end)> codeBlocks)
        {
            return codeBlocks.Any(block => position >= block.start && position < block.end);
        }

        private static void UpdatePosition(Rune rune, ref int line, ref int column, ref int utf16Position)
        {
            if (rune.Value == 0x000A) // LF
            {
                line++;
                column = 1;
            }
            else
            {
                column++;
            }
            utf16Position += rune.Utf16SequenceLength;
        }

        private static string GetContext(string input, int utf16Position, int contextLength)
        {
            var start = Math.Max(0, utf16Position - contextLength);
            var end = Math.Min(input.Length, utf16Position + contextLength);
            return input.Substring(start, end - start);
        }

        private static string GetUnicodeName(int codePoint)
        {
            // This is a simplified implementation
            // In a real application, you would use a proper Unicode database
            return codePoint switch
            {
                0x0000 => "NULL",
                0x0009 => "TAB",
                0x000A => "LINE FEED",
                0x000D => "CARRIAGE RETURN",
                0x00A0 => "NO-BREAK SPACE",
                0x00AD => "SOFT HYPHEN",
                0x200B => "ZERO WIDTH SPACE",
                0x200C => "ZERO WIDTH NON-JOINER",
                0x200D => "ZERO WIDTH JOINER",
                0x200E => "LEFT-TO-RIGHT MARK",
                0x200F => "RIGHT-TO-LEFT MARK",
                0x2028 => "LINE SEPARATOR",
                0x2029 => "PARAGRAPH SEPARATOR",
                0x202F => "NARROW NO-BREAK SPACE",
                0x2060 => "WORD JOINER",
                0xFEFF => "ZERO WIDTH NO-BREAK SPACE",
                _ => $"U+{codePoint:X4}"
            };
        }

        private static string GetZeroWidthMarker(int codePoint) => codePoint switch
        {
            0x200B => "ZWSP",
            0x200C => "ZWNJ",
            0x200D => "ZWJ",
            0x2060 => "WJ",
            0xFEFF => "BOM",
            0x180E => "MVS",
            _ => "ZW"
        };

        private static string GetBiDiMarker(int codePoint) => codePoint switch
        {
            0x200E => "LRM",
            0x200F => "RLM",
            0x202A => "LRE",
            0x202B => "RLE",
            0x202C => "PDF",
            0x202D => "LRO",
            0x202E => "RLO",
            0x2066 => "LRI",
            0x2067 => "RLI",
            0x2068 => "FSI",
            0x2069 => "PDI",
            _ => "BIDI"
        };

        private static string GetMathMarker(int codePoint) => codePoint switch
        {
            0x2062 => "×₀",
            0x2063 => ",₀",
            0x2064 => "+₀",
            _ => "MATH"
        };

        private CharacterDetection CreateCharacterDetection(InvisibleCharacterCategory category, int codePoint, string marker)
        {
            var cleaningActions = CleaningActionFactory.CreateActionsForCharacter(category, codePoint);
            
            return new CharacterDetection
            {
                Category = category,
                CodePoint = codePoint,
                Name = GetUnicodeName(codePoint),
                Marker = marker,
                ReplacementHint = cleaningActions.GetValueOrDefault(CleaningPreset.Safe)?.Description ?? "No action defined",
                CleaningActions = cleaningActions
            };
        }
    }

    public class DetectionResult
    {
        public List<CharacterDetection> DetectedCharacters { get; set; } = new();
        public Dictionary<InvisibleCharacterCategory, int> CategoryCounts { get; set; } = new();
        
        public int TotalCount => DetectedCharacters.Count;
        public bool HasInvisibleCharacters => TotalCount > 0;
    }

    public class CharacterDetection
    {
        public InvisibleCharacterCategory Category { get; set; }
        public int CodePoint { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Marker { get; set; } = string.Empty;
        public string ReplacementHint { get; set; } = string.Empty;
        public int Position { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }
        public string Context { get; set; } = string.Empty;
        
        /// <summary>
        /// Specific cleaning actions for different presets
        /// </summary>
        public Dictionary<CleaningPreset, CleaningAction> CleaningActions { get; set; } = new();
        
        public string CodePointString => $"U+{CodePoint:X4}";
        public string FullName => $"{Name} · {CodePointString}";
        public string Tooltip => $"{FullName} · Action: {ReplacementHint}";
        
        /// <summary>
        /// Gets the appropriate cleaning action for a specific preset
        /// </summary>
        public CleaningAction GetCleaningAction(CleaningPreset preset)
        {
            if (CleaningActions.TryGetValue(preset, out var action))
                return action;
            
            // Fallback to Safe preset if available
            if (preset != CleaningPreset.Safe && CleaningActions.TryGetValue(CleaningPreset.Safe, out var safeAction))
                return safeAction;
            
            // Ultimate fallback - keep character
            return CleaningAction.Keep("No specific action defined");
        }
    }

}