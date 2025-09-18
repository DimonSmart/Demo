using Demo.Services;
using Xunit;

namespace DemoTests.InvisibleCharacters
{
    public class InvisibleCharacterCleanerServiceTests
    {
        private readonly InvisibleCharacterCleanerService _cleaner = new();

        [Fact]
        public void CleanText_EmptyString_ReturnsEmpty()
        {
            var result = _cleaner.CleanText("", CleaningPreset.Safe);
            
            Assert.Equal("", result.CleanedText);
            Assert.False(result.HasChanges);
            Assert.Equal("No changes made.", result.Summary);
        }

        [Fact]
        public void CleanText_NormalText_NoChanges()
        {
            var input = "This is normal text with regular spaces.";
            var result = _cleaner.CleanText(input, CleaningPreset.Safe);
            
            Assert.Equal(input, result.CleanedText);
            Assert.False(result.HasChanges);
        }

        [Theory]
        [InlineData(CleaningPreset.Safe)]
        [InlineData(CleaningPreset.Aggressive)]
        [InlineData(CleaningPreset.ASCIIStrict)]
        [InlineData(CleaningPreset.TypographySoft)]
        [InlineData(CleaningPreset.RTLSafe)]
        [InlineData(CleaningPreset.SEOPlain)]
        public void CleanText_C0C1Controls_RemovesInAllPresets(CleaningPreset preset)
        {
            var input = "Text\u0001with\u007Fcontrol\u0080chars";
            var result = _cleaner.CleanText(input, preset);
            
            Assert.Equal("Textwithcontrolchars", result.CleanedText);
            Assert.True(result.HasChanges);
        }

        [Theory]
        [InlineData("\r\n", "\n")] // CRLF -> LF
        [InlineData("\r", "\n")] // CR -> LF
        [InlineData("\u0085", "\n")] // NEL -> LF
        [InlineData("\u2028", "\n")] // LS -> LF
        [InlineData("\u2029", "\n")] // PS -> LF
        public void CleanText_LineBreaks_NormalizesToLF(string input, string expected)
        {
            var result = _cleaner.CleanText(input, CleaningPreset.Safe);
            
            Assert.Equal(expected, result.CleanedText);
            Assert.True(result.HasChanges);
        }

        [Fact]
        public void CleanText_Tab_ReplacesWithSpaces()
        {
            var input = "Text\tWith\tTabs";
            var result = _cleaner.CleanText(input, CleaningPreset.Safe);
            
            Assert.Equal("Text    With    Tabs", result.CleanedText);
            Assert.True(result.HasChanges);
        }

        [Theory]
        [InlineData("\u2002", " ")] // EN SPACE
        [InlineData("\u2003", " ")] // EM SPACE
        [InlineData("\u3000", " ")] // IDEOGRAPHIC SPACE
        public void CleanText_WideSpaces_ReplacesWithRegularSpace(string input, string expected)
        {
            var result = _cleaner.CleanText(input, CleaningPreset.Safe);
            
            Assert.Equal(expected, result.CleanedText);
            Assert.True(result.HasChanges);
        }

        [Fact]
        public void CleanText_NoBreakSpaces_SafePresetKeeps()
        {
            var input = "Text\u00A0with\u202FNBSP";
            var result = _cleaner.CleanText(input, CleaningPreset.Safe);
            
            // Safe preset should keep NBSP/NNBSP
            Assert.Equal(input, result.CleanedText);
            Assert.False(result.HasChanges);
        }

        [Fact]
        public void CleanText_NoBreakSpaces_AggressivePresetReplaces()
        {
            var input = "Text\u00A0with\u202FNBSP";
            var result = _cleaner.CleanText(input, CleaningPreset.Aggressive);
            
            // Aggressive preset should replace with regular spaces
            Assert.Equal("Text with NBSP", result.CleanedText);
            Assert.True(result.HasChanges);
        }

        [Theory]
        [InlineData("\u200B")] // ZWSP
        [InlineData("\u200C")] // ZWNJ
        [InlineData("\u200D")] // ZWJ
        [InlineData("\u2060")] // WORD JOINER
        [InlineData("\uFEFF")] // BOM
        public void CleanText_ZeroWidthFormat_Removes(string invisibleChar)
        {
            var input = $"Text{invisibleChar}WithInvisible";
            var result = _cleaner.CleanText(input, CleaningPreset.Safe);
            
            Assert.Equal("TextWithInvisible", result.CleanedText);
            Assert.True(result.HasChanges);
        }

        [Theory]
        [InlineData("\u200E")] // LRM
        [InlineData("\u200F")] // RLM
        [InlineData("\u202A")] // LRE
        [InlineData("\u202C")] // PDF
        public void CleanText_BiDiControls_SafePresetRemoves(string bidiChar)
        {
            var input = $"Text{bidiChar}WithBiDi";
            var result = _cleaner.CleanText(input, CleaningPreset.Safe);
            
            Assert.Equal("TextWithBiDi", result.CleanedText);
            Assert.True(result.HasChanges);
        }

        [Fact]
        public void CleanText_BiDiIsolates_RTLSafePresetKeeps()
        {
            var input = "Text\u2066with\u2069isolates"; // LRI + PDI
            var result = _cleaner.CleanText(input, CleaningPreset.RTLSafe);
            
            // RTL-Safe should keep isolates but remove embeddings
            Assert.Equal(input, result.CleanedText);
            Assert.False(result.HasChanges);
        }

        [Fact]
        public void CleanText_SoftHyphen_Removes()
        {
            var input = "soft\u00ADhyphen";
            var result = _cleaner.CleanText(input, CleaningPreset.Safe);
            
            Assert.Equal("softhyphen", result.CleanedText);
            Assert.True(result.HasChanges);
        }

        [Theory]
        [InlineData("\u2062")] // INVISIBLE TIMES
        [InlineData("\u2063")] // INVISIBLE SEPARATOR
        [InlineData("\u2064")] // INVISIBLE PLUS
        public void CleanText_InvisibleMath_ReplacesWithSpace(string mathChar)
        {
            var input = $"Math{mathChar}Formula";
            var result = _cleaner.CleanText(input, CleaningPreset.Safe);
            
            Assert.Equal("Math Formula", result.CleanedText);
            Assert.True(result.HasChanges);
        }

        [Fact]
        public void CleanText_VariationSelectors_SafePresetKeeps()
        {
            var input = "Text\uFE00WithVS";
            var result = _cleaner.CleanText(input, CleaningPreset.Safe);
            
            // Safe preset should keep variation selectors
            Assert.Equal(input, result.CleanedText);
            Assert.False(result.HasChanges);
        }

        [Fact]
        public void CleanText_VariationSelectors_AggressivePresetRemoves()
        {
            var input = "Text\uFE00WithVS";
            var result = _cleaner.CleanText(input, CleaningPreset.Aggressive);
            
            // Aggressive preset should remove variation selectors
            Assert.Equal("TextWithVS", result.CleanedText);
            Assert.True(result.HasChanges);
        }

        [Fact]
        public void CleanText_CombiningMarksWithBase_Keeps()
        {
            var input = "a\u0301"; // a + combining acute accent
            var result = _cleaner.CleanText(input, CleaningPreset.Safe);
            
            // Should keep combining mark with valid base
            Assert.Equal(input, result.CleanedText);
            Assert.False(result.HasChanges);
        }

        [Fact]
        public void CleanText_OrphanedCombiningMark_Removes()
        {
            var input = "\u0301orphaned"; // orphaned combining mark
            var result = _cleaner.CleanText(input, CleaningPreset.Safe);
            
            // Should remove orphaned combining mark
            Assert.Equal("orphaned", result.CleanedText);
            Assert.True(result.HasChanges);
        }

        [Theory]
        [InlineData("\u201C", "\"", CleaningPreset.ASCIIStrict)] // Left quote -> ASCII
        [InlineData("\u2013", "-", CleaningPreset.Aggressive)]   // En dash -> hyphen
        [InlineData("\u0430", "a", CleaningPreset.SEOPlain)]     // Cyrillic a -> Latin a
        public void CleanText_Confusables_ReplacesCorrectly(string input, string expected, CleaningPreset preset)
        {
            var result = _cleaner.CleanText(input, preset);
            
            Assert.Equal(expected, result.CleanedText);
            Assert.True(result.HasChanges);
        }

        [Fact]
        public void CleanText_Confusables_TypographySoftPresetNormalizes()
        {
            var input = "\u201CHello\u201D \u2013 World"; // Smart quotes and en dash
            var result = _cleaner.CleanText(input, CleaningPreset.TypographySoft);
            
            // Typography-Soft should keep smart quotes and normalize dashes
            Assert.Contains("\u201C", result.CleanedText); // LEFT DOUBLE QUOTATION MARK
            Assert.Contains("\u201D", result.CleanedText); // RIGHT DOUBLE QUOTATION MARK
            Assert.Contains("\u2013", result.CleanedText); // EN DASH
        }

        [Fact]
        public void CleanText_SkipCodeBlocks_IgnoresInvisibleInCode()
        {
            var input = "Text `with\u200Bcode` and\u200Cmore text.";
            var options = new CleaningOptions { SkipCodeBlocks = true };
            var result = _cleaner.CleanText(input, CleaningPreset.Safe, options);
            
            // Should clean outside code blocks but not inside
            Assert.Equal("Text `with\u200Bcode` andmore text.", result.CleanedText);
            Assert.True(result.HasChanges);
        }

        [Fact]
        public void CleanText_FencedCodeBlocks_IgnoresInvisibleInCode()
        {
            var input = "Text\n```\ncode\u200Bblock\n```\nwith\u200Cinvisible";
            var options = new CleaningOptions { SkipCodeBlocks = true };
            var result = _cleaner.CleanText(input, CleaningPreset.Safe, options);
            
            // Should preserve invisible characters in fenced code blocks
            Assert.Contains("\u200B", result.CleanedText); // In code block
            Assert.False(result.CleanedText.Contains('\u200C')); // Outside code block, should be removed
        }

        [Fact]
        public void CleanText_MixedInvisibleCharacters_CleansAll()
        {
            var input = "\u0001Text\u00A0with\u200B\u200Emixed\u00ADchars\u2062end";
            var result = _cleaner.CleanText(input, CleaningPreset.Aggressive);
            
            // Should clean all invisible characters according to aggressive preset
            Assert.Equal("Text with mixedchars end", result.CleanedText);
            Assert.True(result.HasChanges);
            Assert.Contains("characters", result.Summary.ToLower());
        }

        [Fact]
        public void CleanText_Statistics_CalculatesCorrectly()
        {
            var input = "\u0001\u0001\u200B\u200E"; // 2 controls + 1 ZWSP + 1 BiDi
            var result = _cleaner.CleanText(input, CleaningPreset.Safe);
            
            Assert.Equal(4, result.Statistics.TotalRemoved);
            Assert.Equal(2, result.Statistics.RemovedCounts[InvisibleCharacterCategory.C0C1Controls]);
            Assert.Equal(1, result.Statistics.RemovedCounts[InvisibleCharacterCategory.ZeroWidthFormat]);
            Assert.Equal(1, result.Statistics.RemovedCounts[InvisibleCharacterCategory.BiDiControls]);
        }

        [Fact]
        public void CleanText_CustomTabSize_UsesCorrectSize()
        {
            var input = "Text\tWith\tTabs";
            var options = new CleaningOptions { TabSize = 2 };
            var result = _cleaner.CleanText(input, CleaningPreset.Safe, options);
            
            Assert.Equal("Text  With  Tabs", result.CleanedText);
            Assert.True(result.HasChanges);
        }

        [Theory]
        [InlineData(CleaningPreset.Safe, "Safe")]
        [InlineData(CleaningPreset.Aggressive, "Aggressive")]
        [InlineData(CleaningPreset.ASCIIStrict, "ASCII-Strict")]
        [InlineData(CleaningPreset.TypographySoft, "Typography-Soft")]
        [InlineData(CleaningPreset.RTLSafe, "RTL-Safe")]
        [InlineData(CleaningPreset.SEOPlain, "SEO/Plain")]
        public void CleanText_AllPresets_Work(CleaningPreset preset, string presetName)
        {
            var input = "\u0001\u200B\u00A0"; // Control + ZWSP + NBSP
            var result = _cleaner.CleanText(input, preset);
            
            // All presets should at least remove control characters and ZWSP
            // Using alternative assertion method due to xUnit issue with DoesNotContain for these characters
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
                // Other presets convert NBSP to regular space
                Assert.DoesNotContain("\u00A0", result.CleanedText);
                Assert.Contains(" ", result.CleanedText);
            }
            
            Assert.Equal(preset, result.Preset);
        }
    }
}