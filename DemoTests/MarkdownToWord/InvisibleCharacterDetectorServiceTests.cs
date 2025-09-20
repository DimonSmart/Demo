using Demo.Demos.MarkdownToWord;
using Xunit;

namespace DemoTests.MarkdownToWord
{
    public class InvisibleCharacterDetectorServiceTests
    {
        private readonly InvisibleCharacterDetectorService _detector = new();

        [Fact]
        public void DetectsC0Controls()
        {
            var input = "test\u0001data";
            var result = _detector.DetectInvisibleCharacters(input);
            
            Assert.True(result.HasInvisibleCharacters);
            Assert.Equal(1, result.TotalCount);
            Assert.Equal(InvisibleCharacterCategory.C0C1Controls, result.DetectedCharacters[0].Category);
            Assert.Equal(0x0001, result.DetectedCharacters[0].CodePoint);
        }

        [Fact]
        public void DetectsC1Controls()
        {
            var input = "test\u0081data";
            var result = _detector.DetectInvisibleCharacters(input);
            
            Assert.True(result.HasInvisibleCharacters);
            Assert.Equal(1, result.TotalCount);
            Assert.Equal(InvisibleCharacterCategory.C0C1Controls, result.DetectedCharacters[0].Category);
            Assert.Equal(0x0081, result.DetectedCharacters[0].CodePoint);
        }

        [Theory]
        [InlineData("\u2028", InvisibleCharacterCategory.LineBreaks)]  // Line Separator
        [InlineData("\u2029", InvisibleCharacterCategory.LineBreaks)]  // Paragraph Separator
        [InlineData("\u000D", InvisibleCharacterCategory.LineBreaks)]  // CR
        [InlineData("\u0085", InvisibleCharacterCategory.LineBreaks)]  // NEL
        public void DetectsLineBreaks(string character, InvisibleCharacterCategory expectedCategory)
        {
            var input = $"text{character}more";
            var result = _detector.DetectInvisibleCharacters(input);
            
            Assert.True(result.HasInvisibleCharacters);
            Assert.Equal(expectedCategory, result.DetectedCharacters[0].Category);
        }

        [Fact]
        public void DetectsTab()
        {
            var input = "left\tright";
            var result = _detector.DetectInvisibleCharacters(input);
            
            Assert.True(result.HasInvisibleCharacters);
            Assert.Equal(InvisibleCharacterCategory.Tab, result.DetectedCharacters[0].Category);
            Assert.Equal(0x0009, result.DetectedCharacters[0].CodePoint);
        }

        [Theory]
        [InlineData("\u2000")] // EN QUAD
        [InlineData("\u2001")] // EM QUAD
        [InlineData("\u2002")] // EN SPACE
        [InlineData("\u2003")] // EM SPACE
        [InlineData("\u2004")] // THREE-PER-EM SPACE
        [InlineData("\u2005")] // FOUR-PER-EM SPACE
        [InlineData("\u2006")] // SIX-PER-EM SPACE
        [InlineData("\u2007")] // FIGURE SPACE
        [InlineData("\u2008")] // PUNCTUATION SPACE
        [InlineData("\u2009")] // THIN SPACE
        [InlineData("\u200A")] // HAIR SPACE
        [InlineData("\u205F")] // MEDIUM MATHEMATICAL SPACE
        [InlineData("\u3000")] // IDEOGRAPHIC SPACE
        public void DetectsWideSpaces(string space)
        {
            var input = $"word{space}word";
            var result = _detector.DetectInvisibleCharacters(input);
            
            Assert.True(result.HasInvisibleCharacters);
            Assert.Equal(InvisibleCharacterCategory.WideSpaces, result.DetectedCharacters[0].Category);
        }

        [Theory]
        [InlineData("\u00A0")] // NBSP
        [InlineData("\u202F")] // NARROW NO-BREAK SPACE
        public void DetectsNoBreakSpaces(string space)
        {
            var input = $"word{space}word";
            var result = _detector.DetectInvisibleCharacters(input);
            
            Assert.True(result.HasInvisibleCharacters);
            Assert.Equal(InvisibleCharacterCategory.NoBreakSpaces, result.DetectedCharacters[0].Category);
        }

        [Theory]
        [InlineData("\u200B")] // ZWSP
        [InlineData("\u200C")] // ZWNJ
        [InlineData("\u200D")] // ZWJ
        [InlineData("\u2060")] // WORD JOINER
        [InlineData("\uFEFF")] // BOM/ZWNBSP
        [InlineData("\u180E")] // MONGOLIAN VOWEL SEPARATOR
        public void DetectsZeroWidthFormat(string character)
        {
            var input = $"text{character}more";
            var result = _detector.DetectInvisibleCharacters(input);
            
            Assert.True(result.HasInvisibleCharacters);
            Assert.Equal(InvisibleCharacterCategory.ZeroWidthFormat, result.DetectedCharacters[0].Category);
        }

        [Theory]
        [InlineData("\u200E")] // LRM
        [InlineData("\u200F")] // RLM
        [InlineData("\u202A")] // LRE
        [InlineData("\u202B")] // RLE
        [InlineData("\u202C")] // PDF
        [InlineData("\u202D")] // LRO
        [InlineData("\u202E")] // RLO
        [InlineData("\u2066")] // LRI
        [InlineData("\u2067")] // RLI
        [InlineData("\u2068")] // FSI
        [InlineData("\u2069")] // PDI
        public void DetectsBiDiControls(string character)
        {
            var input = $"text{character}more";
            var result = _detector.DetectInvisibleCharacters(input);
            
            Assert.True(result.HasInvisibleCharacters);
            Assert.Equal(InvisibleCharacterCategory.BiDiControls, result.DetectedCharacters[0].Category);
        }

        [Fact]
        public void DetectsSoftHyphen()
        {
            var input = "word\u00ADbreak";
            var result = _detector.DetectInvisibleCharacters(input);
            
            Assert.True(result.HasInvisibleCharacters);
            Assert.Equal(InvisibleCharacterCategory.SoftHyphen, result.DetectedCharacters[0].Category);
            Assert.Equal(0x00AD, result.DetectedCharacters[0].CodePoint);
        }

        [Theory]
        [InlineData("\u2062")] // INVISIBLE TIMES
        [InlineData("\u2063")] // INVISIBLE SEPARATOR
        [InlineData("\u2064")] // INVISIBLE PLUS
        public void DetectsInvisibleMath(string character)
        {
            var input = $"math{character}expr";
            var result = _detector.DetectInvisibleCharacters(input);
            
            Assert.True(result.HasInvisibleCharacters);
            Assert.Equal(InvisibleCharacterCategory.InvisibleMath, result.DetectedCharacters[0].Category);
        }

        [Theory]
        [InlineData("\uFE00")] // VS1
        [InlineData("\uFE0F")] // VS16
        public void DetectsVariationSelectors(string character)
        {
            var input = $"text{character}variant";
            var result = _detector.DetectInvisibleCharacters(input);
            
            Assert.True(result.HasInvisibleCharacters);
            Assert.Equal(InvisibleCharacterCategory.VariationSelectors, result.DetectedCharacters[0].Category);
        }

        [Fact]
        public void DetectsVariationSelectorsSupplementary()
        {
            var input = "text\U000E0100variant";
            var result = _detector.DetectInvisibleCharacters(input);
            
            Assert.True(result.HasInvisibleCharacters);
            Assert.Equal(InvisibleCharacterCategory.VariationSelectors, result.DetectedCharacters[0].Category);
            Assert.Equal(0xE0100, result.DetectedCharacters[0].CodePoint);
        }

        [Fact]
        public void DetectsEmojiTags()
        {
            var input = "text\U000E0020tag";
            var result = _detector.DetectInvisibleCharacters(input);
            
            Assert.True(result.HasInvisibleCharacters);
            Assert.Equal(InvisibleCharacterCategory.EmojiTags, result.DetectedCharacters[0].Category);
            Assert.Equal(0xE0020, result.DetectedCharacters[0].CodePoint);
        }

        [Fact]
        public void DetectsCombiningMarks()
        {
            var input = "e\u0301"; // e + combining acute
            var result = _detector.DetectInvisibleCharacters(input);
            
            Assert.True(result.HasInvisibleCharacters);
            Assert.Equal(InvisibleCharacterCategory.CombiningMarks, result.DetectedCharacters[0].Category);
            Assert.Equal(0x0301, result.DetectedCharacters[0].CodePoint);
        }

        [Theory]
        [InlineData("\u201C")] // LEFT DOUBLE QUOTATION MARK
        [InlineData("\u201D")] // RIGHT DOUBLE QUOTATION MARK
        [InlineData("\u2014")] // EM DASH
        [InlineData("\u2212")] // MINUS SIGN
        [InlineData("\u0430")] // CYRILLIC SMALL LETTER A
        public void DetectsConfusables(string character)
        {
            var input = $"text{character}more";
            var result = _detector.DetectInvisibleCharacters(input);
            
            Assert.True(result.HasInvisibleCharacters);
            Assert.Equal(InvisibleCharacterCategory.Confusables, result.DetectedCharacters[0].Category);
        }

        [Fact]
        public void SkipsCodeBlocks()
        {
            var input = "`code\u200Bblock`\n\nNormal\u200Btext";
            var result = _detector.DetectInvisibleCharacters(input, skipCodeBlocks: true);
            
            Assert.True(result.HasInvisibleCharacters);
            Assert.Equal(1, result.TotalCount); // Only the one outside code block
        }

        [Fact]
        public void DoesNotSkipCodeBlocksWhenDisabled()
        {
            var input = "`code\u200Bblock`\n\nNormal\u200Btext";
            var result = _detector.DetectInvisibleCharacters(input, skipCodeBlocks: false);
            
            Assert.True(result.HasInvisibleCharacters);
            Assert.Equal(2, result.TotalCount); // Both should be detected
        }

        [Fact]
        public void ProvidesCategoryCounts()
        {
            var input = "test\u200B\u00A0\u200E\u0001end";
            var result = _detector.DetectInvisibleCharacters(input);
            
            Assert.Equal(4, result.TotalCount);
            Assert.Equal(1, result.CategoryCounts[InvisibleCharacterCategory.ZeroWidthFormat]);
            Assert.Equal(1, result.CategoryCounts[InvisibleCharacterCategory.NoBreakSpaces]);
            Assert.Equal(1, result.CategoryCounts[InvisibleCharacterCategory.BiDiControls]);
            Assert.Equal(1, result.CategoryCounts[InvisibleCharacterCategory.C0C1Controls]);
        }

        [Fact]
        public void ProvidesPositionInformation()
        {
            var input = "ab\u200Bcd";
            var result = _detector.DetectInvisibleCharacters(input);
            
            var detection = result.DetectedCharacters[0];
            Assert.Equal(2, detection.Position); // UTF-16 position
            Assert.Equal(1, detection.Line);
            Assert.Equal(3, detection.Column);
        }

        [Fact]
        public void HandlesEmptyString()
        {
            var result = _detector.DetectInvisibleCharacters("");
            
            Assert.False(result.HasInvisibleCharacters);
            Assert.Equal(0, result.TotalCount);
        }

        [Fact]
        public void HandlesNormalText()
        {
            var input = "This is normal text with no invisible characters.";
            var result = _detector.DetectInvisibleCharacters(input);
            
            Assert.False(result.HasInvisibleCharacters);
            Assert.Equal(0, result.TotalCount);
        }

        [Fact]
        public void ComplexMixedContent()
        {
            var input = "Start\u0001\u200B\u00A0middle\u200E\u2062\uFE0Fend";
            var result = _detector.DetectInvisibleCharacters(input);
            
            Assert.True(result.HasInvisibleCharacters);
            Assert.Equal(6, result.TotalCount);
            
            // Check that all expected categories are present
            var categories = result.DetectedCharacters.Select(d => d.Category).Distinct().ToList();
            Assert.Contains(InvisibleCharacterCategory.C0C1Controls, categories);
            Assert.Contains(InvisibleCharacterCategory.ZeroWidthFormat, categories);
            Assert.Contains(InvisibleCharacterCategory.NoBreakSpaces, categories);
            Assert.Contains(InvisibleCharacterCategory.BiDiControls, categories);
            Assert.Contains(InvisibleCharacterCategory.InvisibleMath, categories);
            Assert.Contains(InvisibleCharacterCategory.VariationSelectors, categories);
        }
    }
}