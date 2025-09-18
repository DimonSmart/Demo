using System.Text;
using System.Web;

namespace Demo.Services
{
    public class InvisibleCharacterVisualizationService
    {
        private readonly InvisibleCharacterDetectorService _detector = new();

        public string VisualizeInvisibleCharacters(string input, VisualizationOptions options)
        {
            if (!options.ShowInvisibleCharacters && !options.ShowLineBreaks)
                return HttpUtility.HtmlEncode(input);

            var detection = _detector.DetectInvisibleCharacters(input, options.SkipCodeBlocks);
            
            // Even if no invisible characters are detected, we might still need to show line breaks
            return ApplyVisualization(input, detection, options);
        }

        private string ApplyVisualization(string input, DetectionResult detection, VisualizationOptions options)
        {
            var sb = new StringBuilder();
            var detectionsByPosition = detection.DetectedCharacters.ToDictionary(d => d.Position, d => d);
            var codeBlockRanges = options.SkipCodeBlocks ? FindCodeBlockRanges(input) : new List<(int start, int end)>();
            
            int position = 0;
            int runeIndex = 0;
            
            foreach (var rune in input.EnumerateRunes())
            {
                // Skip visualization if in code block but still HTML encode
                if (options.SkipCodeBlocks && IsInCodeBlock(position, codeBlockRanges))
                {
                    sb.Append(HttpUtility.HtmlEncode(rune.ToString()));
                    position += rune.Utf16SequenceLength;
                    runeIndex++;
                    continue;
                }

                if (detectionsByPosition.TryGetValue(position, out var charDetection))
                {
                    var visualized = VisualizeCharacter(charDetection, rune, options);
                    sb.Append(visualized);
                }
                else if (options.ShowLineBreaks && rune.Value == 0x000A) // LF
                {
                    // Show line break marker even if LF is not detected as invisible character
                    var cssClass = GetCssClass(InvisibleCharacterCategory.LineBreaks, options.HighlightMode);
                    var marker = "¶";
                    var tooltip = "Line Feed (LF)";
                    var codePoint = "U+000A";
                    var name = "LINE FEED";
                    
                    sb.Append($"<span class=\"{cssClass}\" data-code=\"{codePoint}\" data-name=\"{name}\" title=\"{tooltip}\">{marker}</span>\n");
                }
                else
                {
                    // Regular character - HTML encode for safety
                    sb.Append(HttpUtility.HtmlEncode(rune.ToString()));
                }

                position += rune.Utf16SequenceLength;
                runeIndex++;
            }

            return sb.ToString();
        }

        private string VisualizeCharacter(CharacterDetection detection, Rune rune, VisualizationOptions options)
        {
            var cssClass = GetCssClass(detection.Category, options.HighlightMode);
            var marker = GetVisualMarker(detection, rune);
            var tooltip = HttpUtility.HtmlAttributeEncode(detection.Tooltip);
            var codePoint = detection.CodePointString;
            var name = HttpUtility.HtmlAttributeEncode(detection.Name);

            // Special handling for line breaks - show marker at end of line
            if (detection.Category == InvisibleCharacterCategory.LineBreaks)
            {
                if (rune.Value == 0x000A) // LF
                {
                    return options.ShowLineBreaks 
                        ? $"<span class=\"{cssClass}\" data-code=\"{codePoint}\" data-name=\"{name}\" title=\"{tooltip}\">{marker}</span>\n"
                        : "\n";
                }
                else
                {
                    // CR, NEL, LS, PS - show marker and convert to LF
                    return options.ShowLineBreaks 
                        ? $"<span class=\"{cssClass}\" data-code=\"{codePoint}\" data-name=\"{name}\" title=\"{tooltip}\">{marker}</span>\n"
                        : "\n";
                }
            }

            // Special handling for TAB - show marker with visual tab alignment
            if (detection.Category == InvisibleCharacterCategory.Tab)
            {
                var tabSpaces = new string('\u00A0', 3); // Use NBSP for spacing
                return $"<span class=\"{cssClass}\" data-code=\"{codePoint}\" data-name=\"{name}\" title=\"{tooltip}\">{marker}{tabSpaces}</span>";
            }

            // For all other invisible characters, wrap in span with marker
            return $"<span class=\"{cssClass}\" data-code=\"{codePoint}\" data-name=\"{name}\" title=\"{tooltip}\">⟦{marker}⟧</span>";
        }

        private string GetVisualMarker(CharacterDetection detection, Rune rune)
        {
            return detection.Category switch
            {
                InvisibleCharacterCategory.C0C1Controls => detection.Marker,
                InvisibleCharacterCategory.LineBreaks => "¶",
                InvisibleCharacterCategory.Tab => "→",
                InvisibleCharacterCategory.WideSpaces => "·",
                InvisibleCharacterCategory.NoBreakSpaces => detection.CodePoint == 0x00A0 ? "⍽" : "⍽ⁿ",
                InvisibleCharacterCategory.ZeroWidthFormat => detection.Marker,
                InvisibleCharacterCategory.BiDiControls => detection.Marker,
                InvisibleCharacterCategory.SoftHyphen => "¬",
                InvisibleCharacterCategory.InvisibleMath => detection.Marker,
                InvisibleCharacterCategory.VariationSelectors => detection.Marker,
                InvisibleCharacterCategory.EmojiTags => detection.Marker,
                InvisibleCharacterCategory.CombiningMarks => "◌",
                InvisibleCharacterCategory.Confusables => "≈",
                _ => detection.CodePointString
            };
        }

        private string GetCssClass(InvisibleCharacterCategory category, HighlightMode mode)
        {
            var baseClass = "inv-char";
            var categoryClass = mode == HighlightMode.ByCategory ? $"inv-{GetCategoryName(category).ToLower()}" : "inv-all";
            return $"{baseClass} {categoryClass}";
        }

        private string GetCategoryName(InvisibleCharacterCategory category)
        {
            return category switch
            {
                InvisibleCharacterCategory.C0C1Controls => "control",
                InvisibleCharacterCategory.LineBreaks => "linebreak",
                InvisibleCharacterCategory.Tab => "tab",
                InvisibleCharacterCategory.WideSpaces => "widespace",
                InvisibleCharacterCategory.NoBreakSpaces => "nbsp",
                InvisibleCharacterCategory.ZeroWidthFormat => "zerowidth",
                InvisibleCharacterCategory.BiDiControls => "bidi",
                InvisibleCharacterCategory.SoftHyphen => "softhyphen",
                InvisibleCharacterCategory.InvisibleMath => "invismath",
                InvisibleCharacterCategory.VariationSelectors => "varsel",
                InvisibleCharacterCategory.EmojiTags => "emotag",
                InvisibleCharacterCategory.CombiningMarks => "combining",
                InvisibleCharacterCategory.Confusables => "confusable",
                _ => "unknown"
            };
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

        private bool IsInCodeBlock(int position, List<(int start, int end)> codeBlocks) => codeBlocks.Any(block => position >= block.start && position < block.end);

        public string GenerateVisualizationCSS()
        {
            return @"
.inv-char {
    position: relative;
    border-radius: 2px;
    font-family: 'Courier New', monospace;
    font-size: 0.85em;
    font-weight: bold;
    padding: 0 2px;
    margin: 0 1px;
    display: inline-block;
    cursor: help;
}

/* Category-specific colors */
.inv-control { background-color: rgba(255, 99, 99, 0.3); color: #cc0000; }
.inv-linebreak { background-color: rgba(99, 255, 99, 0.3); color: #006600; }
.inv-tab { background-color: rgba(99, 99, 255, 0.3); color: #000066; }
.inv-widespace { background-color: rgba(255, 255, 99, 0.3); color: #666600; }
.inv-nbsp { background-color: rgba(255, 165, 0, 0.3); color: #cc6600; }
.inv-zerowidth { background-color: rgba(255, 99, 255, 0.3); color: #990099; }
.inv-bidi { background-color: rgba(255, 192, 203, 0.3); color: #990066; }
.inv-softhyphen { background-color: rgba(192, 192, 192, 0.3); color: #666666; }
.inv-invismath { background-color: rgba(144, 238, 144, 0.3); color: #006633; }
.inv-varsel { background-color: rgba(173, 216, 230, 0.3); color: #336699; }
.inv-emotag { background-color: rgba(221, 160, 221, 0.3); color: #663366; }
.inv-combining { background-color: rgba(255, 218, 185, 0.3); color: #994400; }
.inv-confusable { background-color: rgba(255, 140, 0, 0.3); color: #cc4400; }

/* All same color mode */
.inv-all { background-color: rgba(255, 182, 193, 0.4); color: #990033; }

/* Hover effects */
.inv-char:hover {
    background-color: rgba(0, 0, 0, 0.1);
    transform: scale(1.1);
    z-index: 1000;
}

/* Tooltip styling */
.inv-char[title]:hover::after {
    content: attr(title);
    position: absolute;
    bottom: 100%;
    left: 50%;
    transform: translateX(-50%);
    background: #333;
    color: white;
    padding: 4px 8px;
    border-radius: 4px;
    font-size: 12px;
    white-space: nowrap;
    z-index: 1001;
    margin-bottom: 4px;
    box-shadow: 0 2px 8px rgba(0,0,0,0.3);
}

.inv-char[title]:hover::before {
    content: '';
    position: absolute;
    bottom: 100%;
    left: 50%;
    transform: translateX(-50%);
    border: 4px solid transparent;
    border-top-color: #333;
    z-index: 1001;
}

/* Tab visualization specific styling */
.inv-tab {
    min-width: 2em;
    text-align: left;
}

/* Line break visualization */
.inv-linebreak {
    position: absolute;
    right: -1em;
    font-size: 0.8em;
}";
        }
    }

    public class VisualizationOptions
    {
        public bool ShowInvisibleCharacters { get; set; } = false;
        public bool SkipCodeBlocks { get; set; } = true;
        public bool ShowLineBreaks { get; set; } = true;
        public HighlightMode HighlightMode { get; set; } = HighlightMode.ByCategory;
        public HashSet<InvisibleCharacterCategory> EnabledCategories { get; set; } = new();

        public static VisualizationOptions Default => new()
        {
            EnabledCategories = Enum.GetValues<InvisibleCharacterCategory>().ToHashSet()
        };
    }

    public enum HighlightMode
    {
        ByCategory,
        AllSame
    }
}