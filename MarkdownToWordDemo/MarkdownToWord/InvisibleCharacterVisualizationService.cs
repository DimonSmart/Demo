using System.Text;
using System.Web;

namespace Demo.Demos.MarkdownToWord
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

            var position = 0;

            foreach (var rune in input.EnumerateRunes())
            {
                if (options.SkipCodeBlocks && IsInCodeBlock(position, codeBlockRanges))
                {
                    sb.Append(HttpUtility.HtmlEncode(rune.ToString()));
                    position += rune.Utf16SequenceLength;
                    continue;
                }

                if (detectionsByPosition.TryGetValue(position, out var charDetection))
                {
                    if (ShouldVisualizeDetection(charDetection, options))
                    {
                        var visualized = VisualizeCharacter(charDetection, rune, options);
                        sb.Append(visualized);
                    }
                    else
                    {
                        sb.Append(HttpUtility.HtmlEncode(rune.ToString()));
                    }
                }
                else
                {
                    sb.Append(HttpUtility.HtmlEncode(rune.ToString()));
                }

                position += rune.Utf16SequenceLength;
            }

            return sb.ToString();
        }

        private static bool ShouldVisualizeDetection(CharacterDetection detection, VisualizationOptions options)
        {
            if (!IsCategoryEnabled(detection.Category, options))
                return false;

            if (detection.Category == InvisibleCharacterCategory.LineBreaks)
                return options.ShowLineBreaks || options.ShowInvisibleCharacters;

            return options.ShowInvisibleCharacters;
        }

        private static bool IsCategoryEnabled(InvisibleCharacterCategory category, VisualizationOptions options)
        {
            if (options.EnabledCategories is null)
                return true;

            if (options.EnabledCategories.Count == 0)
                return false;

            return options.EnabledCategories.Contains(category);
        }

        private string VisualizeCharacter(CharacterDetection detection, Rune rune, VisualizationOptions options)
        {
            var cssClass = GetCssClass(detection.Category);
            var marker = GetVisualMarker(detection, rune);
            var tooltip = HttpUtility.HtmlAttributeEncode(detection.Tooltip);
            var codePoint = detection.CodePointString;
            var name = HttpUtility.HtmlAttributeEncode(detection.Name);

            // Special handling for line breaks - show marker at end of line
            if (detection.Category == InvisibleCharacterCategory.LineBreaks)
            {
                return options.ShowLineBreaks
                    ? $"<span class=\"{cssClass}\" data-code=\"{codePoint}\" data-name=\"{name}\" title=\"{tooltip}\">{marker}</span>\n"
                    : "\n";
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

        private string GetCssClass(InvisibleCharacterCategory category)
        {
            var baseClass = "inv-char";
            var categoryClass = $"inv-{GetCategoryName(category).ToLower()}";
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
            var inInlineCode = false;
            var inFencedCode = false;
            var codeStart = 0;

            for (var i = 0; i < input.Length; i++)
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

    }

    public class VisualizationOptions
    {
        public bool ShowInvisibleCharacters { get; set; } = false;
        public bool SkipCodeBlocks { get; set; } = true;
        public bool ShowLineBreaks { get; set; } = true;
        public HashSet<InvisibleCharacterCategory> EnabledCategories { get; set; } =
            Enum.GetValues<InvisibleCharacterCategory>().ToHashSet();

        public static VisualizationOptions Default => new();
    }
}
