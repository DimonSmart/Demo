using Demo.Demos.MarkdownToWord;

namespace Demo.Test
{
    /// <summary>
    /// Simple test to verify the new invisible character detection and cleaning architecture
    /// </summary>
    public static class InvisibleCharacterTests
    {
        public static void RunTests()
        {
            Console.WriteLine("Testing new invisible character architecture...\n");

            var detector = new InvisibleCharacterDetectorService();
            var cleaner = new InvisibleCharacterCleanerService();

            // Test 1: Detection
            var testText = "Hello\u200BWo\u00A0rld\u2013test\u200C";
            Console.WriteLine($"Test text: '{testText}'");
            Console.WriteLine("Hex: " + string.Join(" ", testText.Select(c => $"U+{(int)c:X4}")));
            
            var detectionResult = detector.DetectInvisibleCharacters(testText);
            Console.WriteLine($"\nDetected {detectionResult.TotalCount} invisible characters:");
            
            foreach (var detection in detectionResult.DetectedCharacters)
            {
                Console.WriteLine($"  - {detection.FullName} at position {detection.Position} ({detection.Marker})");
                Console.WriteLine($"    Safe action: {detection.GetCleaningAction(CleaningPreset.Safe).ActionType} - {detection.GetCleaningAction(CleaningPreset.Safe).Description}");
                Console.WriteLine($"    Aggressive action: {detection.GetCleaningAction(CleaningPreset.Aggressive).ActionType} - {detection.GetCleaningAction(CleaningPreset.Aggressive).Description}");
                Console.WriteLine();
            }

            // Test 2: Cleaning with different presets
            Console.WriteLine("\n--- Cleaning Tests ---");

            var presets = new[] { CleaningPreset.Safe, CleaningPreset.Aggressive, CleaningPreset.ASCIIStrict };
            
            foreach (var preset in presets)
            {
                var cleaningResult = cleaner.CleanText(testText, preset);
                Console.WriteLine($"\n{preset} preset:");
                Console.WriteLine($"  Original: '{cleaningResult.OriginalText}'");
                Console.WriteLine($"  Cleaned:  '{cleaningResult.CleanedText}'");
                Console.WriteLine($"  Summary:  {cleaningResult.GenerateSummary()}");
            }

            // Test 3: Selective cleaning
            Console.WriteLine("\n--- Selective Cleaning Test ---");
            var categories = new HashSet<InvisibleCharacterCategory> 
            { 
                InvisibleCharacterCategory.ZeroWidthFormat,
                InvisibleCharacterCategory.Confusables
            };

            var selectiveResult = cleaner.CleanSelectedCategories(testText, categories);
            Console.WriteLine($"Selective cleaning (ZeroWidthFormat + Confusables):");
            Console.WriteLine($"  Original: '{selectiveResult.OriginalText}'");
            Console.WriteLine($"  Cleaned:  '{selectiveResult.CleanedText}'");
            Console.WriteLine($"  Summary:  {selectiveResult.GenerateSummary()}");

            Console.WriteLine("\nAll tests completed!");
        }
    }
}