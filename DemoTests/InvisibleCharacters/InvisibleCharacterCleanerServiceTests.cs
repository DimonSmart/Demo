using Xunit;
using Demo.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DemoTests.InvisibleCharacters
{
    public class InvisibleCharacterCleanerServiceTests
    {
        private readonly InvisibleCharacterCleanerService _cleaner;

        public InvisibleCharacterCleanerServiceTests()
        {
            _cleaner = new InvisibleCharacterCleanerService();
        }

        // Individual unit tests removed - functionality is covered by comprehensive UI behavior simulation tests below

        [Theory]
        [InlineData(CleaningPreset.Safe)]
        [InlineData(CleaningPreset.Aggressive)]
        [InlineData(CleaningPreset.ASCIIStrict)]
        [InlineData(CleaningPreset.TypographySoft)]
        [InlineData(CleaningPreset.RTLSafe)]
        [InlineData(CleaningPreset.SEOPlain)]
        public void CleanText_AllPresets_Work(CleaningPreset preset)
        {
            var input = "\u0001\u200B\u00A0"; // Control + ZWSP + NBSP
            var result = _cleaner.CleanText(input, preset);
            
            // All presets should at least remove control characters and ZWSP
            Assert.False(result.CleanedText.Contains('\u0001')); // Control should be removed
            Assert.False(result.CleanedText.Contains('\u200B')); // ZWSP should be removed
            
            // NBSP handling varies by preset
            if (preset == CleaningPreset.Safe || preset == CleaningPreset.TypographySoft || preset == CleaningPreset.RTLSafe)
            {
                // These presets keep NBSP
                Assert.Contains("\u00A0", result.CleanedText);
            }
            else
            {
                // These presets replace NBSP with regular space
                Assert.Contains(" ", result.CleanedText);
            }
            
            Assert.True(result.HasChanges);
        }

        [Fact]
        public void CleanSelectedCategories_ComprehensiveUIOrder_AllCategoriesWork()
        {
            // Arrange - comprehensive test with categories in UI enum order
            var cleaner = new InvisibleCharacterCleanerService();
            var detector = new InvisibleCharacterDetectorService();
            
            // Test text with invisible characters in UI category order (matching enum InvisibleCharacterCategory)
            var testText = string.Join("", new[]
            {
                "Text",                      // Normal text start
                "\u0001",                   // C0C1Controls: SOH
                "\r\n",                     // LineBreaks: CRLF
                "\t",                       // Tab: TAB
                "\u3000",                   // WideSpaces: IDEOGRAPHIC SPACE
                "\u00A0",                   // NoBreakSpaces: NBSP
                "\u200B",                   // ZeroWidthFormat: ZWSP
                "\u200E",                   // BiDiControls: LRM
                "\u00AD",                   // SoftHyphen: SHY
                "2\u2063x",                 // InvisibleMath: INVISIBLE SEPARATOR - should be REMOVED (2x, not 2 x)
                "\uFE0E",                   // VariationSelectors: TEXT PRESENTATION SELECTOR
                "\U000E0020",               // EmojiTags: TAG SPACE
                "a\u0301",                  // CombiningMarks: COMBINING ACUTE ACCENT with base
                "word\u2014end"              // Confusables: EM DASH
            });
            
            var options = new CleaningOptions 
            { 
                SkipCodeBlocks = true, 
                TabSize = 4, 
                InvisibleMathToSpace = true // This should be ignored - InvisibleMath should always be removed in selective cleaning
            };

            // Act - test each category individually (simulating user clicking through categories)
            var allCategories = Enum.GetValues<InvisibleCharacterCategory>();
            
            foreach (var category in allCategories)
            {
                var selectedCategories = new HashSet<InvisibleCharacterCategory> { category };
                var result = cleaner.CleanSelectedCategories(testText, selectedCategories, options);
                
                // Should be able to process each category (some might not have changes if no characters of that type)
                Assert.NotNull(result);
                Assert.NotNull(result.CleanedText);
                
                // Specific verification for InvisibleMath category
                if (category == InvisibleCharacterCategory.InvisibleMath)
                {
                    if (result.HasChanges)
                    {
                        // Should be removed (2x), not replaced with space (2 x)
                        Assert.False(result.CleanedText.Contains('\u2063'));
                        Assert.Contains("2x", result.CleanedText);
                        Assert.False(result.CleanedText.Contains("2 x"));
                    }
                }
            }
            
            // Test all categories together
            var allSelected = new HashSet<InvisibleCharacterCategory>(allCategories);
            var finalResult = cleaner.CleanSelectedCategories(testText, allSelected, options);
            
            Assert.True(finalResult.HasChanges);
            // Final text should have most invisible characters processed
            Assert.False(finalResult.CleanedText.Contains('\u0001')); // C0C1Controls removed
            Assert.False(finalResult.CleanedText.Contains('\u200B')); // ZeroWidth removed
            Assert.False(finalResult.CleanedText.Contains('\u2063')); // InvisibleMath removed (not spaced)
            Assert.Contains("2x", finalResult.CleanedText); // Should be directly concatenated
            Assert.Contains("-", finalResult.CleanedText); // EM DASH converted to hyphen
        }

        [Fact]
        public void CleanSelectedCategories_DetectionTest_VerifyCategories()
        {
            var detector = new InvisibleCharacterDetectorService();
            var cleaner = new InvisibleCharacterCleanerService();

            // Test detection consistency
            var testText = "Text\u0001\u200B\u00AD";
            var detectionResult = detector.DetectInvisibleCharacters(testText);
            
            var categoriesWithCharacters = detectionResult.DetectedCharacters
                .Select(d => d.Category)
                .ToHashSet();
            
            // Should detect C0C1Controls, ZeroWidthFormat, SoftHyphen
            Assert.Contains(InvisibleCharacterCategory.C0C1Controls, categoriesWithCharacters);
            Assert.Contains(InvisibleCharacterCategory.ZeroWidthFormat, categoriesWithCharacters);
            Assert.Contains(InvisibleCharacterCategory.SoftHyphen, categoriesWithCharacters);
            
            // Clean with detected categories
            var cleanResult = cleaner.CleanSelectedCategories(testText, categoriesWithCharacters, new CleaningOptions());
            Assert.True(cleanResult.HasChanges);
            Assert.Equal("Text", cleanResult.CleanedText);
        }

        [Fact]
        public void CleanSelectedCategories_WithSpecificCategories_CleansOnlySelected()
        {
            var testText = "Text\u0001\u200B\u00A0More"; // Control + ZWSP + NBSP
            
            // Only clean C0C1Controls
            var c0c1Only = new HashSet<InvisibleCharacterCategory> { InvisibleCharacterCategory.C0C1Controls };
            var result1 = _cleaner.CleanSelectedCategories(testText, c0c1Only, new CleaningOptions());
            
            Assert.True(result1.HasChanges);
            Assert.False(result1.CleanedText.Contains('\u0001')); // Control removed
            Assert.True(result1.CleanedText.Contains('\u200B')); // ZWSP preserved
            Assert.True(result1.CleanedText.Contains('\u00A0')); // NBSP preserved
            
            // Only clean ZeroWidthFormat
            var zwspOnly = new HashSet<InvisibleCharacterCategory> { InvisibleCharacterCategory.ZeroWidthFormat };
            var result2 = _cleaner.CleanSelectedCategories(testText, zwspOnly, new CleaningOptions());
            
            Assert.True(result2.HasChanges);
            Assert.True(result2.CleanedText.Contains('\u0001')); // Control preserved
            Assert.False(result2.CleanedText.Contains('\u200B')); // ZWSP removed
            Assert.True(result2.CleanedText.Contains('\u00A0')); // NBSP preserved
        }

        [Fact]
        public void CleanSelectedCategories_WithMultipleCategories_CleansAllSelected()
        {
            var testText = "Text\u0001\u200B\u00A0\u2013More"; // Control + ZWSP + NBSP + En Dash
            
            var multipleCategories = new HashSet<InvisibleCharacterCategory>
            {
                InvisibleCharacterCategory.C0C1Controls,
                InvisibleCharacterCategory.ZeroWidthFormat,
                InvisibleCharacterCategory.Confusables
            };
            
            var result = _cleaner.CleanSelectedCategories(testText, multipleCategories, new CleaningOptions());
            
            Assert.True(result.HasChanges);
            Assert.False(result.CleanedText.Contains('\u0001')); // Control removed
            Assert.False(result.CleanedText.Contains('\u200B')); // ZWSP removed
            Assert.True(result.CleanedText.Contains('\u00A0')); // NBSP preserved (not selected)
            Assert.True(result.CleanedText.Contains('-')); // En dash converted to hyphen
            Assert.False(result.CleanedText.Contains('\u2013')); // Original en dash removed
        }

        [Fact]
        public void CleanSelectedCategories_UIBehaviorSimulation_SelectiveCleaning()
        {
            // Comprehensive user scenario simulation - user selectively deletes categories
            var initialText = string.Join("", new[]
            {
                "Example text with various invisible characters:",
                "\u0001", // C0C1Controls: SOH  
                "\u00A0", // NoBreakSpaces: NBSP
                "\u200B", // ZeroWidthFormat: ZWSP
                "\u00AD", // SoftHyphen: SHY
                "2\u2063x", // InvisibleMath: INVISIBLE SEPARATOR - should be removed, not replaced with space
                "\u200E", // BiDiControls: LRM
                "smart\u2014quote" // Confusables: EM DASH
            });

            var options = new CleaningOptions { SkipCodeBlocks = true, TabSize = 4, InvisibleMathToSpace = false };

            // Step 1: User deletes C0C1Controls first
            var step1Categories = new HashSet<InvisibleCharacterCategory> 
            { 
                InvisibleCharacterCategory.C0C1Controls 
            };
            var step1Result = _cleaner.CleanSelectedCategories(initialText, step1Categories, options);

            Assert.True(step1Result.HasChanges);
            Assert.False(step1Result.CleanedText.Contains('\u0001')); // SOH removed
            Assert.Contains("\u00A0", step1Result.CleanedText); // NBSP still there
            Assert.Contains("\u200B", step1Result.CleanedText); // ZWSP still there
            
            // Step 2: User then deletes ZeroWidthFormat (building on step 1)
            var step2Categories = new HashSet<InvisibleCharacterCategory> 
            { 
                InvisibleCharacterCategory.C0C1Controls,
                InvisibleCharacterCategory.ZeroWidthFormat
            };
            var step2Result = _cleaner.CleanSelectedCategories(initialText, step2Categories, options);
            
            Assert.False(step2Result.CleanedText.Contains('\u0001')); // SOH removed
            Assert.False(step2Result.CleanedText.Contains('\u200B')); // ZWSP removed
            Assert.Contains("\u00A0", step2Result.CleanedText); // NBSP still there
            Assert.Contains("\u00AD", step2Result.CleanedText); // SoftHyphen still there

            // Step 3: User adds InvisibleMath category
            var step3Categories = new HashSet<InvisibleCharacterCategory> 
            { 
                InvisibleCharacterCategory.C0C1Controls,
                InvisibleCharacterCategory.ZeroWidthFormat,
                InvisibleCharacterCategory.InvisibleMath
            };
            var step3Result = _cleaner.CleanSelectedCategories(initialText, step3Categories, options);
            
            // Verify InvisibleMath is removed, not replaced with space
            Assert.False(step3Result.CleanedText.Contains('\u2063')); // INVISIBLE SEPARATOR removed
            Assert.Contains("2x", step3Result.CleanedText); // Should be directly concatenated
            Assert.False(step3Result.CleanedText.Contains("2 x")); // Should NOT have space

            // Step 4: User adds Confusables (em dash test)
            var step4Categories = new HashSet<InvisibleCharacterCategory> 
            { 
                InvisibleCharacterCategory.C0C1Controls,
                InvisibleCharacterCategory.ZeroWidthFormat,
                InvisibleCharacterCategory.InvisibleMath,
                InvisibleCharacterCategory.Confusables
            };
            var step4Result = _cleaner.CleanSelectedCategories(initialText, step4Categories, options);
            
            // Verify em dash conversion
            Assert.False(step4Result.CleanedText.Contains('\u2014')); // EM DASH removed
            Assert.Contains("smart-quote", step4Result.CleanedText); // EM DASH converted to hyphen
            
            // Final verification - all selected categories processed
            Assert.False(step4Result.CleanedText.Contains('\u0001')); // C0C1Controls
            Assert.False(step4Result.CleanedText.Contains('\u200B')); // ZeroWidthFormat
            Assert.False(step4Result.CleanedText.Contains('\u2063')); // InvisibleMath
            Assert.False(step4Result.CleanedText.Contains('\u2014')); // Confusables
            
            // But preserved non-selected categories
            Assert.Contains("\u00A0", step4Result.CleanedText); // NoBreakSpaces preserved
            Assert.Contains("\u00AD", step4Result.CleanedText); // SoftHyphen preserved
            Assert.Contains("\u200E", step4Result.CleanedText); // BiDiControls preserved
        }

        [Fact]
        public void CleanSelectedCategories_Confusables_EmDashToHyphen()
        {
            // Test the specific issue: "Confusable dash: word—dash → word-dash"
            var testCases = new[]
            {
                ("word\u2014dash", "word-dash"), // EM DASH (U+2014)
                ("word\u2013dash", "word-dash"), // EN DASH (U+2013)
                ("word\u2212dash", "word-dash"), // MINUS SIGN (U+2212)
                ("multiple\u2014dashes\u2013here\u2212test", "multiple-dashes-here-test")
            };

            var confusablesCategory = new HashSet<InvisibleCharacterCategory> 
            { 
                InvisibleCharacterCategory.Confusables 
            };

            foreach (var (input, expected) in testCases)
            {
                var result = _cleaner.CleanSelectedCategories(input, confusablesCategory, new CleaningOptions());
                
                Assert.True(result.HasChanges, $"Expected changes for input: {input}");
                Assert.Equal(expected, result.CleanedText);
                
                // Verify original characters are gone
                Assert.False(result.CleanedText.Contains('\u2014')); // EM DASH
                Assert.False(result.CleanedText.Contains('\u2013')); // EN DASH  
                Assert.False(result.CleanedText.Contains('\u2212')); // MINUS SIGN
                
                // Verify replacement character is present
                Assert.Contains("-", result.CleanedText);
            }
        }

        [Fact]
        public void CleanSelectedCategories_Confusables_DetectionTest()
        {
            var detector = new InvisibleCharacterDetectorService();
            var testText = "word\u2014dash";
            
            var detectionResult = detector.DetectInvisibleCharacters(testText);
            
            // Should detect confusables category
            var confusableDetections = detectionResult.DetectedCharacters
                .Where(d => d.Category == InvisibleCharacterCategory.Confusables)
                .ToList();
            
            Assert.Single(confusableDetections); // One confusable character
            Assert.Equal('\u2014', (char)confusableDetections[0].CodePoint); // EM DASH
        }

        [Theory]
        [InlineData("word\u2014dash", "word-dash")] // EM DASH
        [InlineData("word\u2013dash", "word-dash")] // EN DASH
        [InlineData("word\u2212dash", "word-dash")] // MINUS SIGN
        public void CleanSelectedCategories_Confusables_AllDashesToHyphen(string input, string expected)
        {
            var confusablesOnly = new HashSet<InvisibleCharacterCategory> 
            { 
                InvisibleCharacterCategory.Confusables 
            };
            
            var result = _cleaner.CleanSelectedCategories(input, confusablesOnly, new CleaningOptions());
            
            Assert.True(result.HasChanges);
            Assert.Equal(expected, result.CleanedText);
        }

        [Fact]
        public void CleanSelectedCategories_RealWorldExample_DashesToHyphens()
        {
            // Real world example from user feedback
            var realWorldText = "This is a test with em dash — and en dash – and minus sign − characters.";
            var expected = "This is a test with em dash - and en dash - and minus sign - characters.";
            
            var confusablesOnly = new HashSet<InvisibleCharacterCategory> 
            { 
                InvisibleCharacterCategory.Confusables 
            };
            
            var result = _cleaner.CleanSelectedCategories(realWorldText, confusablesOnly, new CleaningOptions());
            
            Assert.True(result.HasChanges);
            Assert.Equal(expected, result.CleanedText);
            
            // Verify specific replacements
            Assert.False(result.CleanedText.Contains("—")); // EM DASH gone
            Assert.False(result.CleanedText.Contains("–")); // EN DASH gone
            Assert.False(result.CleanedText.Contains("−")); // MINUS SIGN gone
            Assert.Contains("-", result.CleanedText); // Regular hyphen present
        }
    }
}