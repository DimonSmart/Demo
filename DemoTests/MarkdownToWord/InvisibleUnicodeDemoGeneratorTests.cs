using Demo.Demos.MarkdownToWord;
using Xunit;

namespace DemoTests.MarkdownToWord
{
    public class InvisibleUnicodeDemoGeneratorTests
    {
        [Fact]
        public void GeneratesValidMarkdown()
        {
            var markdown = InvisibleUnicodeDemoGenerator.BuildMarkdown();
            
            Assert.NotNull(markdown);
            Assert.NotEmpty(markdown);
            Assert.Contains("**Comprehensive invisible character test", markdown);
        }

        [Fact]
        public void ContainsAllMainCategories()
        {
            var markdown = InvisibleUnicodeDemoGenerator.BuildMarkdown();
            
            // Check that demo contains examples of major categories
            Assert.Contains("C0 Control character", markdown);
            Assert.Contains("C1 Control character", markdown);
            Assert.Contains("Line separator", markdown);
            Assert.Contains("Paragraph separator", markdown);
            Assert.Contains("Tab character", markdown);
            Assert.Contains("EN QUAD", markdown);
            Assert.Contains("EM QUAD", markdown);
            Assert.Contains("THIN SPACE", markdown);
            Assert.Contains("IDEOGRAPHIC SPACE", markdown);
            Assert.Contains("Non-breaking space", markdown);
            Assert.Contains("Narrow no-break space", markdown);
            Assert.Contains("Zero-width space", markdown);
            Assert.Contains("Zero-width joiner", markdown);
            Assert.Contains("Word joiner", markdown);
            Assert.Contains("BOM/ZWNBSP", markdown);
            Assert.Contains("Left-to-right mark", markdown);
            Assert.Contains("Right-to-left mark", markdown);
            Assert.Contains("LR embedding", markdown);
            Assert.Contains("LR isolate", markdown);
            Assert.Contains("Soft hyphen", markdown);
            Assert.Contains("Invisible times", markdown);
            Assert.Contains("Invisible separator", markdown);
            Assert.Contains("Invisible plus", markdown);
            Assert.Contains("Variation selector", markdown);
            Assert.Contains("TAG space", markdown);
            Assert.Contains("Emoji tag sequence", markdown);
            Assert.Contains("Combining acute", markdown);
        }

        [Fact]
        public void IncludesConfusablesSection()
        {
            var markdown = InvisibleUnicodeDemoGenerator.BuildMarkdown();
            
            Assert.Contains("**Confusable/suspicious characters", markdown);
            Assert.Contains("Em dash", markdown);
            Assert.Contains("Smart quotes", markdown);
            Assert.Contains("Mixed alphabet word", markdown);
            Assert.Contains("Minus sign", markdown);
        }

        [Fact]
        public void GeneratedTextContainsInvisibleCharacters()
        {
            var markdown = InvisibleUnicodeDemoGenerator.BuildMarkdown();
            var detector = new InvisibleCharacterDetectorService();
            var result = detector.DetectInvisibleCharacters(markdown, skipCodeBlocks: false);
            
            // The demo should contain many invisible characters for testing
            Assert.True(result.HasInvisibleCharacters);
            Assert.True(result.TotalCount > 20, $"Expected more than 20 invisible chars, got {result.TotalCount}");
            
            // Should contain examples from multiple categories
            Assert.True(result.CategoryCounts.Count >= 10, 
                $"Expected at least 10 different categories, got {result.CategoryCounts.Count}");
        }

        [Fact]
        public void CleanerCanProcessDemoText()
        {
            var markdown = InvisibleUnicodeDemoGenerator.BuildMarkdown();
            var cleaned = InvisibleCleaner.Clean(markdown);
            
            // Cleaned version should be shorter (invisible chars removed/normalized)
            Assert.True(cleaned.Length < markdown.Length, 
                $"Cleaned length ({cleaned.Length}) should be less than original ({markdown.Length})");
            
            // But should still contain visible content
            Assert.Contains("Comprehensive invisible character test", cleaned);
            Assert.Contains("Confusable/suspicious characters", cleaned);
        }

        [Fact]
        public void DemoCoversExtendedUnicodeRanges()
        {
            var markdown = InvisibleUnicodeDemoGenerator.BuildMarkdown();
            
            // Test specific Unicode ranges are covered
            
            // Should contain Variation Selectors Supplement (U+E0100-U+E01EF)
            Assert.True(markdown.Contains("\U000E0100") || markdown.EnumerateRunes().Any(rune => rune.Value >= 0xE0100 && rune.Value <= 0xE01EF),
                "Should contain Variation Selectors Supplement");
            
            // Should contain TAG characters (U+E0000-U+E007F)
            Assert.True(markdown.EnumerateRunes().Any(rune => rune.Value >= 0xE0000 && rune.Value <= 0xE007F),
                "Should contain TAG characters");
            
            // Should contain comprehensive space characters
            var spaceChars = new int[] { 0x2000, 0x2001, 0x2002, 0x2003, 0x2004, 0x2005, 
                                       0x2006, 0x2007, 0x2008, 0x2009, 0x200A, 0x205F, 0x3000 };
            foreach (var space in spaceChars)
            {
                Assert.True(markdown.Contains(char.ConvertFromUtf32(space)),
                    $"Should contain space character U+{space:X4}");
            }
        }

        [Fact]
        public void GeneratesConsistentOutput()
        {
            var markdown1 = InvisibleUnicodeDemoGenerator.BuildMarkdown();
            var markdown2 = InvisibleUnicodeDemoGenerator.BuildMarkdown();
            
            Assert.Equal(markdown1, markdown2);
        }

        [Fact]
        public void FormattedAsMarkdownBulletList()
        {
            var markdown = InvisibleUnicodeDemoGenerator.BuildMarkdown();
            var lines = markdown.Split('\n');
            
            var bulletCount = lines.Count(line => line.Trim().StartsWith("- "));
            
            // Debug output
            System.Diagnostics.Debug.WriteLine($"Total lines: {lines.Length}");
            System.Diagnostics.Debug.WriteLine($"Bullet count: {bulletCount}");
            
            Assert.True(bulletCount >= 45, $"Expected at least 45 bullet points, got {bulletCount}. Total lines: {lines.Length}");
            
            // Should have section headers  
            Assert.True(lines.Any(line => line.StartsWith("**") && line.Contains("**")), "Should contain lines with markdown bold formatting");
        }
    }
}