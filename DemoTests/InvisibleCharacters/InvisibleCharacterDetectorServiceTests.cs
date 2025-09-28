using Demo.Demos.MarkdownToWord;

namespace DemoTests.InvisibleCharacters
{
    public class InvisibleCharacterDetectorServiceTests
    {
        private readonly InvisibleCharacterDetectorService _detector = new();

        [Fact]
        public void DetectInvisibleCharacters_EmptyString_ReturnsNoDetections()
        {
            var result = _detector.DetectInvisibleCharacters("", skipCodeBlocks: true);
            
            Assert.Empty(result.DetectedCharacters);
            Assert.False(result.HasInvisibleCharacters);
            Assert.Equal(0, result.TotalCount);
        }

        [Fact]
        public void DetectInvisibleCharacters_NormalText_ReturnsNoDetections()
        {
            var input = "This is normal text with regular spaces and punctuation.";
            var result = _detector.DetectInvisibleCharacters(input, skipCodeBlocks: true);
            
            Assert.Empty(result.DetectedCharacters);
            Assert.False(result.HasInvisibleCharacters);
        }

        [Theory]
        [InlineData("\u0000", InvisibleCharacterCategory.C0C1Controls)] // NULL
        [InlineData("\u0001", InvisibleCharacterCategory.C0C1Controls)] // SOH
        [InlineData("\u007F", InvisibleCharacterCategory.C0C1Controls)] // DEL
        [InlineData("\u0080", InvisibleCharacterCategory.C0C1Controls)] // C1 control
        public void DetectInvisibleCharacters_C0C1Controls_DetectsCorrectly(string input, InvisibleCharacterCategory expectedCategory)
        {
            var result = _detector.DetectInvisibleCharacters(input, skipCodeBlocks: false);
            
            Assert.Single(result.DetectedCharacters);
            Assert.Equal(expectedCategory, result.DetectedCharacters[0].Category);
            Assert.Equal(1, result.CategoryCounts[expectedCategory]);
        }

        [Theory]
        [InlineData("\u000D", InvisibleCharacterCategory.LineBreaks)] // CR
        [InlineData("\u0085", InvisibleCharacterCategory.LineBreaks)] // NEL
        [InlineData("\u2028", InvisibleCharacterCategory.LineBreaks)] // LS
        [InlineData("\u2029", InvisibleCharacterCategory.LineBreaks)] // PS
        public void DetectInvisibleCharacters_LineBreaks_DetectsCorrectly(string input, InvisibleCharacterCategory expectedCategory)
        {
            var result = _detector.DetectInvisibleCharacters(input, skipCodeBlocks: false);
            
            Assert.Single(result.DetectedCharacters);
            Assert.Equal(expectedCategory, result.DetectedCharacters[0].Category);
        }

        [Fact]
        public void DetectInvisibleCharacters_Tab_DetectsCorrectly()
        {
            var input = "Text\tWith\tTabs";
            var result = _detector.DetectInvisibleCharacters(input, skipCodeBlocks: false);
            
            Assert.Equal(2, result.DetectedCharacters.Count);
            Assert.All(result.DetectedCharacters, d => Assert.Equal(InvisibleCharacterCategory.Tab, d.Category));
            Assert.Equal(2, result.CategoryCounts[InvisibleCharacterCategory.Tab]);
        }

        [Theory]
        [InlineData("\u2002", InvisibleCharacterCategory.WideSpaces)] // EN SPACE
        [InlineData("\u2003", InvisibleCharacterCategory.WideSpaces)] // EM SPACE
        [InlineData("\u3000", InvisibleCharacterCategory.WideSpaces)] // IDEOGRAPHIC SPACE
        public void DetectInvisibleCharacters_WideSpaces_DetectsCorrectly(string input, InvisibleCharacterCategory expectedCategory)
        {
            var result = _detector.DetectInvisibleCharacters(input, skipCodeBlocks: false);
            
            Assert.Single(result.DetectedCharacters);
            Assert.Equal(expectedCategory, result.DetectedCharacters[0].Category);
        }

        [Theory]
        [InlineData("\u00A0", InvisibleCharacterCategory.NoBreakSpaces)] // NBSP
        [InlineData("\u202F", InvisibleCharacterCategory.NoBreakSpaces)] // NNBSP
        public void DetectInvisibleCharacters_NoBreakSpaces_DetectsCorrectly(string input, InvisibleCharacterCategory expectedCategory)
        {
            var result = _detector.DetectInvisibleCharacters(input, skipCodeBlocks: false);
            
            Assert.Single(result.DetectedCharacters);
            Assert.Equal(expectedCategory, result.DetectedCharacters[0].Category);
        }

        [Theory]
        [InlineData("\u200B", InvisibleCharacterCategory.ZeroWidthFormat)] // ZWSP
        [InlineData("\u200C", InvisibleCharacterCategory.ZeroWidthFormat)] // ZWNJ
        [InlineData("\u200D", InvisibleCharacterCategory.ZeroWidthFormat)] // ZWJ
        [InlineData("\u2060", InvisibleCharacterCategory.ZeroWidthFormat)] // WORD JOINER
        [InlineData("\uFEFF", InvisibleCharacterCategory.ZeroWidthFormat)] // BOM
        public void DetectInvisibleCharacters_ZeroWidthFormat_DetectsCorrectly(string input, InvisibleCharacterCategory expectedCategory)
        {
            var result = _detector.DetectInvisibleCharacters(input, skipCodeBlocks: false);
            
            Assert.Single(result.DetectedCharacters);
            Assert.Equal(expectedCategory, result.DetectedCharacters[0].Category);
        }

        [Theory]
        [InlineData("\u200E", InvisibleCharacterCategory.BiDiControls)] // LRM
        [InlineData("\u200F", InvisibleCharacterCategory.BiDiControls)] // RLM
        [InlineData("\u202A", InvisibleCharacterCategory.BiDiControls)] // LRE
        [InlineData("\u2066", InvisibleCharacterCategory.BiDiControls)] // LRI
        public void DetectInvisibleCharacters_BiDiControls_DetectsCorrectly(string input, InvisibleCharacterCategory expectedCategory)
        {
            var result = _detector.DetectInvisibleCharacters(input, skipCodeBlocks: false);
            
            Assert.Single(result.DetectedCharacters);
            Assert.Equal(expectedCategory, result.DetectedCharacters[0].Category);
        }

        [Fact]
        public void DetectInvisibleCharacters_SoftHyphen_DetectsCorrectly()
        {
            var input = "soft\u00ADhyphen"; // SOFT HYPHEN
            var result = _detector.DetectInvisibleCharacters(input, skipCodeBlocks: false);
            
            Assert.Single(result.DetectedCharacters);
            Assert.Equal(InvisibleCharacterCategory.SoftHyphen, result.DetectedCharacters[0].Category);
        }

        [Theory]
        [InlineData("\u2062", InvisibleCharacterCategory.InvisibleMath)] // INVISIBLE TIMES
        [InlineData("\u2063", InvisibleCharacterCategory.InvisibleMath)] // INVISIBLE SEPARATOR
        [InlineData("\u2064", InvisibleCharacterCategory.InvisibleMath)] // INVISIBLE PLUS
        public void DetectInvisibleCharacters_InvisibleMath_DetectsCorrectly(string input, InvisibleCharacterCategory expectedCategory)
        {
            var result = _detector.DetectInvisibleCharacters(input, skipCodeBlocks: false);
            
            Assert.Single(result.DetectedCharacters);
            Assert.Equal(expectedCategory, result.DetectedCharacters[0].Category);
        }

        [Theory]
        [InlineData("\uFE00", InvisibleCharacterCategory.VariationSelectors)] // VS1
        [InlineData("\uFE0F", InvisibleCharacterCategory.VariationSelectors)] // VS16
        public void DetectInvisibleCharacters_VariationSelectors_DetectsCorrectly(string input, InvisibleCharacterCategory expectedCategory)
        {
            var result = _detector.DetectInvisibleCharacters(input, skipCodeBlocks: false);
            
            Assert.Single(result.DetectedCharacters);
            Assert.Equal(expectedCategory, result.DetectedCharacters[0].Category);
        }

        [Theory]
        [InlineData("a\u0301", InvisibleCharacterCategory.CombiningMarks)] // a + COMBINING ACUTE ACCENT
        [InlineData("\u0301", InvisibleCharacterCategory.CombiningMarks)] // Orphaned combining mark
        public void DetectInvisibleCharacters_CombiningMarks_DetectsCorrectly(string input, InvisibleCharacterCategory expectedCategory)
        {
            var result = _detector.DetectInvisibleCharacters(input, skipCodeBlocks: false);
            
            var combiningMarks = result.DetectedCharacters.Where(d => d.Category == expectedCategory).ToList();
            Assert.Single(combiningMarks);
        }

        [Theory]
        [InlineData("\u201C", InvisibleCharacterCategory.Confusables)] // LEFT DOUBLE QUOTATION MARK
        [InlineData("\u201D", InvisibleCharacterCategory.Confusables)] // RIGHT DOUBLE QUOTATION MARK
        [InlineData("\u2013", InvisibleCharacterCategory.Confusables)] // EN DASH
        public void DetectInvisibleCharacters_Confusables_DetectsCorrectly(string input, InvisibleCharacterCategory expectedCategory)
        {
            var result = _detector.DetectInvisibleCharacters(input, skipCodeBlocks: false);

            Assert.Single(result.DetectedCharacters);
            Assert.Equal(expectedCategory, result.DetectedCharacters[0].Category);
        }

        [Fact]
        public void DetectInvisibleCharacters_PureCyrillicWord_DoesNotTriggerConfusable()
        {
            var input = "сосна";
            var result = _detector.DetectInvisibleCharacters(input, skipCodeBlocks: false);

            Assert.DoesNotContain(result.DetectedCharacters, d => d.Category == InvisibleCharacterCategory.Confusables);
        }

        [Fact]
        public void DetectInvisibleCharacters_CyrillicSentence_DoesNotTriggerConfusable()
        {
            var input = "Это пример русского текста и обычных слов.";
            var result = _detector.DetectInvisibleCharacters(input, skipCodeBlocks: false);

            Assert.DoesNotContain(result.DetectedCharacters, d => d.Category == InvisibleCharacterCategory.Confusables);
        }

        [Fact]
        public void DetectInvisibleCharacters_SingleCyrillicLetterInRussianContext_DoesNotTriggerConfusable()
        {
            var input = "И пример русского текста";
            var result = _detector.DetectInvisibleCharacters(input, skipCodeBlocks: false);

            Assert.DoesNotContain(result.DetectedCharacters, d => d.Category == InvisibleCharacterCategory.Confusables);
        }

        [Fact]
        public void DetectInvisibleCharacters_StandaloneCyrillicInEnglishText_DetectsConfusable()
        {
            var input = "This line has letter и among English words.";
            var result = _detector.DetectInvisibleCharacters(input, skipCodeBlocks: false);

            Assert.Contains(result.DetectedCharacters, d => d.Category == InvisibleCharacterCategory.Confusables && d.CodePoint == 0x0438);
        }

        [Fact]
        public void DetectInvisibleCharacters_MixedAlphabetWord_DetectsConfusable()
        {
            var input = "pa\u0441sword";
            var result = _detector.DetectInvisibleCharacters(input, skipCodeBlocks: false);

            Assert.Contains(result.DetectedCharacters, d => d.Category == InvisibleCharacterCategory.Confusables && d.CodePoint == 0x0441);
        }

        [Fact]
        public void DetectInvisibleCharacters_SkipCodeBlocks_IgnoresInvisibleInCode()
        {
            var input = "Text with `\u200B` inline code and:\n```\n\u200C\u200D\n```\nMore text.";
            var result = _detector.DetectInvisibleCharacters(input, skipCodeBlocks: true);
            
            // Should not detect invisible characters inside code blocks
            Assert.Empty(result.DetectedCharacters);
        }

        [Fact]
        public void DetectInvisibleCharacters_DoNotSkipCodeBlocks_DetectsInCode()
        {
            var input = "Text with `\u200B` inline code.";
            var result = _detector.DetectInvisibleCharacters(input, skipCodeBlocks: false);
            
            // Should detect invisible characters inside code blocks
            Assert.Single(result.DetectedCharacters);
            Assert.Equal(InvisibleCharacterCategory.ZeroWidthFormat, result.DetectedCharacters[0].Category);
        }

        [Fact]
        public void DetectInvisibleCharacters_MixedCategories_DetectsAll()
        {
            var input = "\u0001\u00A0\u200B\u200E"; // Control + NBSP + ZWSP + LRM
            var result = _detector.DetectInvisibleCharacters(input, skipCodeBlocks: false);
            
            Assert.Equal(4, result.DetectedCharacters.Count);
            Assert.Equal(4, result.TotalCount);
            Assert.True(result.HasInvisibleCharacters);
            
            // Verify each category is detected
            var categories = result.DetectedCharacters.Select(d => d.Category).ToList();
            Assert.Contains(InvisibleCharacterCategory.C0C1Controls, categories);
            Assert.Contains(InvisibleCharacterCategory.NoBreakSpaces, categories);
            Assert.Contains(InvisibleCharacterCategory.ZeroWidthFormat, categories);
            Assert.Contains(InvisibleCharacterCategory.BiDiControls, categories);
        }

        [Fact]
        public void DetectInvisibleCharacters_PositionTracking_ReturnsCorrectPositions()
        {
            var input = "A\u200BB\nC\u00A0D"; // A + ZWSP + B + LF + C + NBSP + D
            var result = _detector.DetectInvisibleCharacters(input, skipCodeBlocks: false);
            
            Assert.Equal(2, result.DetectedCharacters.Count);
            
            var zwsp = result.DetectedCharacters.First(d => d.Category == InvisibleCharacterCategory.ZeroWidthFormat);
            Assert.Equal(1, zwsp.Position); // Position after 'A'
            Assert.Equal(1, zwsp.Line);
            Assert.Equal(2, zwsp.Column);
            
            var nbsp = result.DetectedCharacters.First(d => d.Category == InvisibleCharacterCategory.NoBreakSpaces);
            Assert.Equal(5, nbsp.Position); // Position after 'C'
            Assert.Equal(2, nbsp.Line);
            Assert.Equal(2, nbsp.Column);
        }
    }
}