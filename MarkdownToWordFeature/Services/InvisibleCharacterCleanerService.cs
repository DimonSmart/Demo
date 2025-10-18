using System.Globalization;
using System.Text;

namespace MarkdownToWordFeature.Services
{
    public class InvisibleCharacterCleanerService
    {
        private readonly InvisibleCharacterDetectorService _detector = new();

        /// <summary>
        /// Cleans invisible characters from text using a specified preset
        /// </summary>
        public CleaningResult CleanText(string input, CleaningPreset preset, CleaningOptions? options = null)
        {
            options ??= GetDefaultOptions(preset);
            
            var detectionResult = _detector.DetectInvisibleCharacters(input, options.SkipCodeBlocks);
            var cleanedText = ApplyCleaningRules(input, preset, options, detectionResult);
            
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

        /// <summary>
        /// Cleans only specific categories of invisible characters from the text
        /// </summary>
        public CleaningResult CleanSelectedCategories(string input, HashSet<InvisibleCharacterCategory> enabledCategories, CleaningOptions? options = null)
        {
            options ??= GetDefaultOptions(CleaningPreset.Safe);
            
            var detectionResult = _detector.DetectInvisibleCharacters(input, options.SkipCodeBlocks);
            var cleanedText = ApplySelectiveCleaning(input, enabledCategories, options, detectionResult);
            
            var afterCleaningResult = _detector.DetectInvisibleCharacters(cleanedText, options.SkipCodeBlocks);
            
            return new CleaningResult
            {
                OriginalText = input,
                CleanedText = cleanedText,
                OriginalDetection = detectionResult,
                AfterCleaningDetection = afterCleaningResult,
                Preset = CleaningPreset.Safe,
                Options = options,
                Statistics = CalculateStatistics(detectionResult, afterCleaningResult)
            };
        }

        private string ApplyCleaningRules(string input, CleaningPreset preset, CleaningOptions options, DetectionResult detectionResult)
        {
            // If no invisible characters found, return original text
            if (!detectionResult.HasInvisibleCharacters)
                return input;
            
            // Create a mapping of positions to character detections for quick lookup
            var positionToDetection = detectionResult.DetectedCharacters
                .ToDictionary(cd => cd.Position, cd => cd);
            
            var sb = new StringBuilder(input.Length);
            var runes = input.EnumerateRunes().ToArray();
            int position = 0;
            
            foreach (var rune in runes)
            {
                if (positionToDetection.TryGetValue(position, out var detection))
                {
                    // This is an invisible character - apply the appropriate cleaning action
                    var cleaningAction = detection.GetCleaningAction(preset);
                    var processedChar = ApplyCleaningAction(rune, cleaningAction, new CleaningContext
                    {
                        OriginalText = input,
                        Position = position,
                        CurrentCharacter = rune,
                        PreviousCharacter = GetPreviousCharacter(runes, position),
                        NextCharacter = GetNextCharacter(runes, position),
                        Preset = preset,
                        Options = options,
                        IsInCodeBlock = false // Already handled by detector when creating DetectionResult
                    });
                    
                    if (processedChar != null)
                        sb.Append(processedChar);
                }
                else
                {
                    // This is a visible character - keep it as is
                    sb.Append(rune.ToString());
                }
                
                position += rune.Utf16SequenceLength;
            }

            return sb.ToString();
        }

        private string ApplySelectiveCleaning(string input, HashSet<InvisibleCharacterCategory> enabledCategories, CleaningOptions options, DetectionResult detectionResult)
        {
            // If no categories are enabled, return original text
            if (enabledCategories.Count == 0)
                return input;
            
            // Build a mapping of positions to character detections for enabled categories only
            var positionsToClean = detectionResult.DetectedCharacters
                .Where(cd => enabledCategories.Contains(cd.Category))
                .ToDictionary(cd => cd.Position, cd => cd);
            
            if (positionsToClean.Count == 0)
                return input;
            
            var runes = input.EnumerateRunes().ToArray();
            var sb = new StringBuilder(input.Length);
            int position = 0;
            
            foreach (var rune in runes)
            {
                if (positionsToClean.TryGetValue(position, out var charDetection))
                {
                    // Use a more aggressive preset for confusables when explicitly selected
                    var presetForAction = charDetection.Category == InvisibleCharacterCategory.Confusables
                        ? CleaningPreset.Aggressive
                        : CleaningPreset.Safe;

                    var cleaningAction = charDetection.GetCleaningAction(presetForAction);
                    var processedChar = ApplyCleaningAction(rune, cleaningAction, new CleaningContext
                    {
                        OriginalText = input,
                        Position = position,
                        CurrentCharacter = rune,
                        PreviousCharacter = GetPreviousCharacter(runes, position),
                        NextCharacter = GetNextCharacter(runes, position),
                        Preset = presetForAction,
                        Options = options,
                        IsInCodeBlock = false
                    });
                    
                    if (processedChar != null)
                        sb.Append(processedChar);
                }
                else
                {
                    // Keep the character as-is
                    sb.Append(rune.ToString());
                }
                
                position += rune.Utf16SequenceLength;
            }
            
            return sb.ToString();
        }

        private string? ApplyCleaningAction(Rune character, CleaningAction action, CleaningContext context)
        {
            return action.ActionType switch
            {
                CleaningActionType.Remove => null,
                CleaningActionType.Keep => character.ToString(),
                CleaningActionType.Replace => action.ReplacementText,
                CleaningActionType.Normalize => action.ReplacementText,
                CleaningActionType.Conditional => action.ConditionalResolver?.Invoke(context),
                _ => character.ToString() // Default fallback
            };
        }

        private Rune? GetPreviousCharacter(Rune[] runes, int position)
        {
            // Find the rune that ends at or before the given position
            int currentPos = 0;
            for (int i = 0; i < runes.Length; i++)
            {
                if (currentPos == position)
                {
                    return i > 0 ? (Rune?)runes[i - 1] : null;
                }
                currentPos += runes[i].Utf16SequenceLength;
            }
            return null;
        }

        private Rune? GetNextCharacter(Rune[] runes, int position)
        {
            // Find the rune that starts at the given position and return the next one
            int currentPos = 0;
            for (int i = 0; i < runes.Length; i++)
            {
                if (currentPos == position)
                {
                    return i < runes.Length - 1 ? (Rune?)runes[i + 1] : null;
                }
                currentPos += runes[i].Utf16SequenceLength;
            }
            return null;
        }

        private CleaningStatistics CalculateStatistics(DetectionResult before, DetectionResult after)
        {
            var stats = new CleaningStatistics();
            
            foreach (var category in Enum.GetValues<InvisibleCharacterCategory>())
            {
                var beforeCount = before.CategoryCounts.GetValueOrDefault(category, 0);
                var afterCount = after.CategoryCounts.GetValueOrDefault(category, 0);
                var removed = beforeCount - afterCount;
                
                if (removed > 0)
                {
                    stats.RemovedCounts[category] = removed;
                    stats.TotalRemoved += removed;
                }
            }
            
            return stats;
        }

        private CleaningOptions GetDefaultOptions(CleaningPreset preset)
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
                    InvisibleMathToSpace = false
                },
                CleaningPreset.ASCIIStrict => new CleaningOptions
                {
                    SkipCodeBlocks = false,
                    TabSize = 4,
                    PreserveZWJZWNJ = false,
                    InvisibleMathToSpace = false
                },
                CleaningPreset.TypographySoft => new CleaningOptions
                {
                    SkipCodeBlocks = true,
                    TabSize = 1,
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
                    SkipCodeBlocks = false,
                    TabSize = 1,
                    PreserveZWJZWNJ = false,
                    InvisibleMathToSpace = false
                },
                _ => new CleaningOptions()
            };
        }
    }

    public class CleaningResult
    {
        public required string OriginalText { get; init; }
        public required string CleanedText { get; init; }
        public required DetectionResult OriginalDetection { get; init; }
        public required DetectionResult AfterCleaningDetection { get; init; }
        public required CleaningPreset Preset { get; init; }
        public required CleaningOptions Options { get; init; }
        public required CleaningStatistics Statistics { get; init; }

        public bool HasChanges => OriginalText != CleanedText;
        public int OriginalLength => OriginalText.Length;
        public int CleanedLength => CleanedText.Length;
        public int CharactersRemoved => OriginalLength - CleanedLength;

        public string GenerateSummary()
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
}