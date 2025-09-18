using System.Globalization;
using System.Text;

namespace Demo.Services
{
    public class InvisibleCharacterDetectorService
    {
        // Character sets for different categories
        private static readonly HashSet<int> C0C1Controls = new()
        {
            // C0 controls (0x00-0x1F except \t \n \r)
            0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 
            0x0B, 0x0C, 0x0E, 0x0F, 0x10, 0x11, 0x12, 0x13, 0x14, 
            0x15, 0x16, 0x17, 0x18, 0x19, 0x1A, 0x1B, 0x1C, 0x1D, 
            0x1E, 0x1F, 0x7F, // DEL
            // C1 controls (0x80-0x9F)
            0x80, 0x81, 0x82, 0x83, 0x84, 0x86, 0x87, 0x88, 0x89, 
            0x8A, 0x8B, 0x8C, 0x8D, 0x8E, 0x8F, 0x90, 0x91, 0x92, 
            0x93, 0x94, 0x95, 0x96, 0x97, 0x98, 0x99, 0x9A, 0x9B, 
            0x9C, 0x9D, 0x9E, 0x9F
        };

        private static readonly HashSet<int> LineBreaks = new()
        {
            // Note: Regular LF (0x000A) is not included as it's a normal text formatting character
            // Only exotic line breaks are considered "invisible characters"
            0x000D, // CR
            0x0085, // NEL
            0x2028, // LS
            0x2029  // PS
        };

        private static readonly HashSet<int> WideSpaces = new()
        {
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

        private static readonly HashSet<int> NoBreakSpaces = new()
        {
            0x00A0, // NBSP
            0x202F  // NNBSP
        };

        private static readonly HashSet<int> ZeroWidthFormat = new()
        {
            0x200B, // ZWSP
            0x200C, // ZWNJ
            0x200D, // ZWJ
            0x2060, // WORD JOINER
            0xFEFF, // BOM/ZWNBSP
            0x180E  // MONGOLIAN VOWEL SEPARATOR (historical)
        };

        private static readonly HashSet<int> BiDiControls = new()
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

        private static readonly HashSet<int> InvisibleMath = new()
        {
            0x2062, // INVISIBLE TIMES
            0x2063, // INVISIBLE SEPARATOR
            0x2064  // INVISIBLE PLUS
        };

        private static readonly HashSet<int> VariationSelectors = new();
        private static readonly HashSet<int> EmojiTags = new();

        // Confusables mapping for category 13
        private static readonly Dictionary<int, string> Confusables = new()
        {
            // Quotes
            { 0x201C, "\"" }, // LEFT DOUBLE QUOTATION MARK → ASCII
            { 0x201D, "\"" }, // RIGHT DOUBLE QUOTATION MARK → ASCII
            { 0x2018, "'" }, // LEFT SINGLE QUOTATION MARK → ASCII
            { 0x2019, "'" }, // RIGHT SINGLE QUOTATION MARK → ASCII

            // Dashes
            { 0x2212, "-" }, // MINUS SIGN → HYPHEN-MINUS
            { 0x2013, "-" }, // EN DASH → HYPHEN-MINUS
            { 0x2014, "-" }, // EM DASH → HYPHEN-MINUS

            // Cyrillic lookalikes
            { 0x0430, "a" }, // CYRILLIC SMALL LETTER A → LATIN
            { 0x0435, "e" }, // CYRILLIC SMALL LETTER IE → LATIN
            { 0x043E, "o" }, // CYRILLIC SMALL LETTER O → LATIN
            { 0x0440, "p" }, // CYRILLIC SMALL LETTER ER → LATIN
            { 0x0441, "c" }, // CYRILLIC SMALL LETTER ES → LATIN
            { 0x0443, "y" }, // CYRILLIC SMALL LETTER U → LATIN
            { 0x0445, "x" }  // CYRILLIC SMALL LETTER HA → LATIN
        };

        static InvisibleCharacterDetectorService()
        {
            // Initialize variation selectors
            for (var i = 0xFE00; i <= 0xFE0F; i++)
                VariationSelectors.Add(i);
            for (var i = 0xE0100; i <= 0xE01EF; i++)
                VariationSelectors.Add(i);

            // Initialize emoji tags
            EmojiTags.Add(0xE0001);
            for (var i = 0xE0020; i <= 0xE007F; i++)
                EmojiTags.Add(i);
        }

        public DetectionResult DetectInvisibleCharacters(string input, bool skipCodeBlocks = true)
        {
            var result = new DetectionResult();
            var codeBlockRanges = skipCodeBlocks ? FindCodeBlockRanges(input) : new List<(int start, int end)>();
            
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

                var detection = ClassifyCharacter(rune);
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

        private CharacterDetection? ClassifyCharacter(Rune rune)
        {
            var codePoint = rune.Value;
            var category = CharUnicodeInfo.GetUnicodeCategory(codePoint);

            // Category 1: ASCII Control C0/C1
            if (C0C1Controls.Contains(codePoint))
            {
                return new CharacterDetection
                {
                    Category = InvisibleCharacterCategory.C0C1Controls,
                    CodePoint = codePoint,
                    Name = GetUnicodeName(codePoint),
                    Marker = "CTRL",
                    ReplacementHint = "remove"
                };
            }

            // Category 2: Line breaks
            if (LineBreaks.Contains(codePoint))
            {
                var hint = codePoint switch
                {
                    0x000D => "normalize CR to CR-LF or remove if before LF",
                    0x0085 => "replace NEL with CR-LF",
                    0x2028 => "replace LS with CR-LF",
                    0x2029 => "replace PS with CR-LF",
                    _ => "normalize to LF"
                };

                return new CharacterDetection
                {
                    Category = InvisibleCharacterCategory.LineBreaks,
                    CodePoint = codePoint,
                    Name = GetUnicodeName(codePoint),
                    Marker = "¶",
                    ReplacementHint = hint
                };
            }

            // Category 3: Tab
            if (codePoint == 0x0009)
            {
                return new CharacterDetection
                {
                    Category = InvisibleCharacterCategory.Tab,
                    CodePoint = codePoint,
                    Name = "TAB",
                    Marker = "→",
                    ReplacementHint = "4 spaces"
                };
            }

            // Category 4: Wide spaces
            if (WideSpaces.Contains(codePoint))
            {
                return new CharacterDetection
                {
                    Category = InvisibleCharacterCategory.WideSpaces,
                    CodePoint = codePoint,
                    Name = GetUnicodeName(codePoint),
                    Marker = "·",
                    ReplacementHint = "regular space"
                };
            }

            // Category 5: Non-breaking spaces
            if (NoBreakSpaces.Contains(codePoint))
            {
                return new CharacterDetection
                {
                    Category = InvisibleCharacterCategory.NoBreakSpaces,
                    CodePoint = codePoint,
                    Name = GetUnicodeName(codePoint),
                    Marker = codePoint == 0x00A0 ? "⍽" : "⍽ⁿ",
                    ReplacementHint = "keep (safe) / regular space (aggressive)"
                };
            }

            // Category 6: Zero-width/format
            if (ZeroWidthFormat.Contains(codePoint))
            {
                return new CharacterDetection
                {
                    Category = InvisibleCharacterCategory.ZeroWidthFormat,
                    CodePoint = codePoint,
                    Name = GetUnicodeName(codePoint),
                    Marker = GetZeroWidthMarker(codePoint),
                    ReplacementHint = "remove"
                };
            }

            // Category 7: BiDi controls
            if (BiDiControls.Contains(codePoint))
            {
                return new CharacterDetection
                {
                    Category = InvisibleCharacterCategory.BiDiControls,
                    CodePoint = codePoint,
                    Name = GetUnicodeName(codePoint),
                    Marker = GetBiDiMarker(codePoint),
                    ReplacementHint = "remove"
                };
            }

            // Category 8: Soft hyphen
            if (codePoint == 0x00AD)
            {
                return new CharacterDetection
                {
                    Category = InvisibleCharacterCategory.SoftHyphen,
                    CodePoint = codePoint,
                    Name = "SOFT HYPHEN",
                    Marker = "¬",
                    ReplacementHint = "remove"
                };
            }

            // Category 9: Invisible math
            if (InvisibleMath.Contains(codePoint))
            {
                return new CharacterDetection
                {
                    Category = InvisibleCharacterCategory.InvisibleMath,
                    CodePoint = codePoint,
                    Name = GetUnicodeName(codePoint),
                    Marker = GetMathMarker(codePoint),
                    ReplacementHint = "regular space / remove"
                };
            }

            // Category 10: Variation selectors
            if (VariationSelectors.Contains(codePoint))
            {
                return new CharacterDetection
                {
                    Category = InvisibleCharacterCategory.VariationSelectors,
                    CodePoint = codePoint,
                    Name = GetUnicodeName(codePoint),
                    Marker = $"VS{(codePoint >= 0xFE00 && codePoint <= 0xFE0F ? codePoint - 0xFE00 + 1 : "S")}",
                    ReplacementHint = "keep (safe) / remove (aggressive)"
                };
            }

            // Category 11: Emoji tags
            if (EmojiTags.Contains(codePoint))
            {
                return new CharacterDetection
                {
                    Category = InvisibleCharacterCategory.EmojiTags,
                    CodePoint = codePoint,
                    Name = GetUnicodeName(codePoint),
                    Marker = "TAG",
                    ReplacementHint = "keep (safe) / remove (aggressive)"
                };
            }

            // Category 12: Combining marks
            if (category == UnicodeCategory.NonSpacingMark)
            {
                return new CharacterDetection
                {
                    Category = InvisibleCharacterCategory.CombiningMarks,
                    CodePoint = codePoint,
                    Name = GetUnicodeName(codePoint),
                    Marker = "◌",
                    ReplacementHint = "remove if orphaned / keep with base"
                };
            }

            // Category 13: Confusables
            if (Confusables.ContainsKey(codePoint))
            {
                return new CharacterDetection
                {
                    Category = InvisibleCharacterCategory.Confusables,
                    CodePoint = codePoint,
                    Name = GetUnicodeName(codePoint),
                    Marker = "≈",
                    ReplacementHint = $"→ {Confusables[codePoint]}"
                };
            }

            return null;
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
        
        public string CodePointString => $"U+{CodePoint:X4}";
        public string FullName => $"{Name} · {CodePointString}";
        public string Tooltip => $"{FullName} · Action: {ReplacementHint}";
    }

    public enum InvisibleCharacterCategory
    {
        C0C1Controls = 1,
        LineBreaks = 2,
        Tab = 3,
        WideSpaces = 4,
        NoBreakSpaces = 5,
        ZeroWidthFormat = 6,
        BiDiControls = 7,
        SoftHyphen = 8,
        InvisibleMath = 9,
        VariationSelectors = 10,
        EmojiTags = 11,
        CombiningMarks = 12,
        Confusables = 13
    }
}