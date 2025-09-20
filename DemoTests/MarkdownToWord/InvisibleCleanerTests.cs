using Demo.Demos.MarkdownToWord;
using System.Text;
using Xunit;

namespace DemoTests.MarkdownToWord
{
    public class InvisibleCleanerTests
    {
        [Theory]
        [InlineData("a\u200Bb", "ab")]                           // ZWSP
        [InlineData("a\u00A0b", "a b")]                          // NBSP -> space
        [InlineData("a\u2009b", "a b")]                          // THIN SPACE -> space
        [InlineData("a\u2000b", "a b")]                          // EN QUAD -> space
        [InlineData("a\u3000b", "a b")]                          // IDEOGRAPHIC SPACE -> space
        [InlineData("x\u200Ex", "xx")]                           // LRM
        [InlineData("x\u200Fx", "xx")]                           // RLM
        [InlineData("x\uFEFFx", "xx")]                           // BOM
        [InlineData("x\u2060x", "xx")]                           // WJ
        [InlineData("x\u2063x", "xx")]                           // Invisible Separator
        [InlineData("x\u2062x", "xx")]                           // Invisible Times
        [InlineData("x\u2064x", "xx")]                           // Invisible Plus
        [InlineData("❤\uFE0F", "❤")]                             // VS-16 removed
        [InlineData("text\uFE00", "text")]                       // VS-1 removed
        [InlineData("a\u202Ab\u202Cc", "abc")]                   // LRE + PDF removed, content stays
        [InlineData("a\u2066b\u2069c", "abc")]                   // LRI + PDI removed, content stays
        [InlineData("x\u00ADx", "xx")]                           // Soft hyphen removed
        [InlineData("a\u2028b", "ab")]                           // Line separator removed
        [InlineData("a\u2029b", "ab")]                           // Paragraph separator removed
        [InlineData("flag\U000E0067\U000E007F", "flag")]    // Only TAGS removed, flag emoji stays
        [InlineData("text\U000E0020end", "textend")]             // TAG SPACE removed
        [InlineData("ctrl\u0001char", "ctrlchar")]               // C0 control removed
        [InlineData("test\u0081data", "testdata")]               // C1 control removed
        public void RemovesInvisibleCharacters(string input, string expected)
        {
            var output = InvisibleCleaner.Clean(input);
            Assert.Equal(expected, output);
        }

        [Fact]
        public void TabsToSpaces()
        {
            var options = new InvisibleCleaner.Options(TabToSpaces: 2);
            Assert.Equal("a  b", InvisibleCleaner.Clean("a\tb", options));
        }

        [Fact]
        public void TabsToSpaces_DefaultFour()
        {
            Assert.Equal("a    b", InvisibleCleaner.Clean("a\tb"));
        }

        [Fact]
        public void KeepsTabsWhenConfigured()
        {
            var options = new InvisibleCleaner.Options(TabToSpaces: 0);
            Assert.Equal("a\tb", InvisibleCleaner.Clean("a\tb", options));
        }

        [Fact]
        public void KeepsCrLfWhenConfigured()
        {
            var options = new InvisibleCleaner.Options(RemoveControlChars: true, KeepCrLf: true);
            Assert.Equal("a\r\nb", InvisibleCleaner.Clean("a\r\nb", options));
        }

        [Fact]
        public void RemovesCrLfWhenConfigured()
        {
            var options = new InvisibleCleaner.Options(RemoveControlChars: true, KeepCrLf: false);
            Assert.Equal("ab", InvisibleCleaner.Clean("a\r\nb", options));
        }

        [Fact]
        public void PreservesZwjZwnjWhenConfigured()
        {
            var options = new InvisibleCleaner.Options(PreserveZWJZWNJ: true);
            Assert.Equal("a\u200Cb\u200Dc", InvisibleCleaner.Clean("a\u200Cb\u200Dc", options));
        }

        [Fact]
        public void RemovesZwjZwnjByDefault()
        {
            Assert.Equal("abc", InvisibleCleaner.Clean("a\u200Cb\u200Dc"));
        }

        [Fact]
        public void InvisibleMathToSpace()
        {
            var options = new InvisibleCleaner.Options(InvisibleMathToSpace: true);
            Assert.Equal("a b c", InvisibleCleaner.Clean("a\u2062b\u2063c", options));
        }

        [Fact]
        public void InvisibleMathRemoved()
        {
            var options = new InvisibleCleaner.Options(InvisibleMathToSpace: false);
            Assert.Equal("abc", InvisibleCleaner.Clean("a\u2062b\u2063c", options));
        }

        [Fact]
        public void WhitelistPreventsRemoval()
        {
            var whitelist = new HashSet<int> { 0x200B }; // ZWSP
            var options = new InvisibleCleaner.Options(WhitelistedCharacters: whitelist);
            Assert.Equal("a\u200Bb", InvisibleCleaner.Clean("a\u200Bb", options));
        }

        [Fact]
        public void BlacklistForcesRemoval()
        {
            var blacklist = new HashSet<int> { 0x0020 }; // Regular space
            var options = new InvisibleCleaner.Options(BlacklistedCharacters: blacklist);
            Assert.Equal("ab", InvisibleCleaner.Clean("a b", options));
        }

        [Fact]
        public void UnicodeNormalization()
        {
            // Test that NFC normalization works
            var decomposed = "e\u0301"; // e + combining acute
            var composed = "é";         // precomposed é
            
            var options = new InvisibleCleaner.Options(NormalizeUnicode: true);
            var result = InvisibleCleaner.Clean(decomposed, options);
            
            Assert.Equal(composed, result);
        }

        [Fact]
        public void SkipsNormalizationWhenDisabled()
        {
            var decomposed = "e\u0301"; // e + combining acute
            var options = new InvisibleCleaner.Options(NormalizeUnicode: false);
            var result = InvisibleCleaner.Clean(decomposed, options);
            
            // Should remove combining mark but not normalize
            Assert.Equal("e", result);
        }

        [Fact]
        public void HandlesEmptyString()
        {
            Assert.Equal("", InvisibleCleaner.Clean(""));
        }

        [Fact]
        public void HandlesNullString()
        {
            Assert.Equal("", InvisibleCleaner.Clean(null!));
        }

        [Fact]
        public void GetCleaningStats_Basic()
        {
            var original = "a\u200Bb\u00A0c";
            var cleaned = InvisibleCleaner.Clean(original);
            var stats = InvisibleCleaner.GetCleaningStats(original, cleaned);

            Assert.True(stats.HasChanges);
            Assert.Equal(5, stats.OriginalLength);
            Assert.Equal(4, stats.CleanedLength); // "a b c" (NBSP becomes space)
            Assert.Equal(1, stats.CharactersRemoved); // Only ZWSP removed, NBSP normalized to space
            Assert.Equal(1, stats.ZeroWidthRemoved);
            Assert.Equal(1, stats.WeirdSpacesNormalized);
        }

        [Fact]
        public void GetCleaningStats_NoChanges()
        {
            var text = "normal text";
            var stats = InvisibleCleaner.GetCleaningStats(text, text);

            Assert.False(stats.HasChanges);
            Assert.Equal(0, stats.CharactersRemoved);
        }

        [Fact]
        public void GetCleaningStats_Summary()
        {
            var original = "a\u200Bb\u00A0c\u200Ed";
            var cleaned = InvisibleCleaner.Clean(original);
            var stats = InvisibleCleaner.GetCleaningStats(original, cleaned);

            var summary = stats.GetSummary();
            Assert.Contains("2 characters", summary);  // Only ZWSP and LRM removed, NBSP normalized to space
            Assert.Contains("1 zero-width", summary);
            Assert.Contains("1 BiDi controls", summary);
        }

        [Theory]
        [InlineData("test", "test")]
        [InlineData("a\u200Bb", "ab")]
        [InlineData("keep\u00A0together", "keep together")]
        [InlineData("mixed\u200B\u00A0\u2009text", "mixed  text")]
        public void ComprehensiveCleaningTests(string input, string expected)
        {
            var result = InvisibleCleaner.Clean(input);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ExtensiveUnicodeTest()
        {
            // Test various categories
            var builder = new StringBuilder();
            builder.Append("start");
            builder.Append('\u0001');        // C0 control
            builder.Append('\u200B');        // ZWSP
            builder.Append('\u2000');        // EN QUAD
            builder.Append('\u200E');        // LRM
            builder.Append('\u2062');        // Invisible times
            builder.Append('\uFE0F');        // Variation selector
            builder.Append("\U000E0020");    // TAG space
            builder.Append("end");

            var input = builder.ToString();
            var result = InvisibleCleaner.Clean(input);
            
            Assert.Equal("start end", result);
        }
    }
}