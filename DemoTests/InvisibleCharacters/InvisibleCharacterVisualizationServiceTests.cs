using Demo.Services;
using Xunit;

namespace DemoTests.InvisibleCharacters
{
    public class InvisibleCharacterVisualizationServiceTests
    {
        private readonly InvisibleCharacterVisualizationService _visualizer = new();

        [Fact]
        public void VisualizeInvisibleCharacters_EmptyString_ReturnsEmpty()
        {
            var options = new VisualizationOptions { ShowInvisibleCharacters = true };
            var result = _visualizer.VisualizeInvisibleCharacters("", options);
            
            Assert.Equal("", result);
        }

        [Fact]
        public void VisualizeInvisibleCharacters_ShowDisabled_ReturnsHtmlEncoded()
        {
            var input = "Text with <tag> & special chars";
            var options = new VisualizationOptions { ShowInvisibleCharacters = false };
            var result = _visualizer.VisualizeInvisibleCharacters(input, options);
            
            Assert.Contains("&lt;tag&gt;", result);
            Assert.Contains("&amp;", result);
        }

        [Fact]
        public void VisualizeInvisibleCharacters_NormalText_ReturnsHtmlEncoded()
        {
            var input = "Normal text with no invisible characters";
            var options = new VisualizationOptions { ShowInvisibleCharacters = true };
            var result = _visualizer.VisualizeInvisibleCharacters(input, options);
            
            Assert.Equal("Normal text with no invisible characters", result);
        }

        [Fact]
        public void VisualizeInvisibleCharacters_ControlCharacter_WrapsInSpan()
        {
            var input = "Text\u0001WithControl";
            var options = new VisualizationOptions { ShowInvisibleCharacters = true };
            var result = _visualizer.VisualizeInvisibleCharacters(input, options);
            
            Assert.Contains("<span", result);
            Assert.Contains("inv-char", result);
            Assert.Contains("inv-control", result);
            Assert.Contains("CTRL", result);
            Assert.Contains("U+0001", result);
            Assert.Contains("title=", result);
        }

        [Fact]
        public void VisualizeInvisibleCharacters_Tab_ShowsTabMarker()
        {
            var input = "Text\tWithTab";
            var options = new VisualizationOptions { ShowInvisibleCharacters = true };
            var result = _visualizer.VisualizeInvisibleCharacters(input, options);
            
            Assert.Contains("inv-tab", result);
            Assert.Contains("→", result);
            Assert.Contains("U+0009", result);
        }

        [Fact]
        public void VisualizeInvisibleCharacters_LineBreak_ShowsAtEndOfLine()
        {
            var input = "Line1\nLine2";
            var options = new VisualizationOptions 
            { 
                ShowInvisibleCharacters = true, 
                ShowLineBreaks = true 
            };
            var result = _visualizer.VisualizeInvisibleCharacters(input, options);
            
            // Should show line break marker and actual newline
            Assert.Contains("¶", result);
            Assert.Contains("\n", result);
            Assert.Contains("inv-linebreak", result);
        }

        [Fact]
        public void VisualizeInvisibleCharacters_LineBreakDisabled_NoMarker()
        {
            var input = "Line1\nLine2";
            var options = new VisualizationOptions 
            { 
                ShowInvisibleCharacters = true, 
                ShowLineBreaks = false 
            };
            var result = _visualizer.VisualizeInvisibleCharacters(input, options);
            
            // Should not show line break marker
            Assert.DoesNotContain("¶", result);
            Assert.Contains("\n", result); // But should still have the actual newline
        }

        [Fact]
        public void VisualizeInvisibleCharacters_WideSpace_ShowsMarker()
        {
            var input = "Text\u2003WithEmSpace";
            var options = new VisualizationOptions { ShowInvisibleCharacters = true };
            var result = _visualizer.VisualizeInvisibleCharacters(input, options);
            
            Assert.Contains("inv-widespace", result);
            Assert.Contains("·", result);
            Assert.Contains("U+2003", result);
        }

        [Fact]
        public void VisualizeInvisibleCharacters_NBSP_ShowsSpecialMarker()
        {
            var input = "Text\u00A0WithNBSP";
            var options = new VisualizationOptions { ShowInvisibleCharacters = true };
            var result = _visualizer.VisualizeInvisibleCharacters(input, options);
            
            Assert.Contains("inv-nbsp", result);
            Assert.Contains("⍽", result);
            Assert.Contains("U+00A0", result);
        }

        [Fact]
        public void VisualizeInvisibleCharacters_ZeroWidth_ShowsMarker()
        {
            var input = "Text\u200BWithZWSP";
            var options = new VisualizationOptions { ShowInvisibleCharacters = true };
            var result = _visualizer.VisualizeInvisibleCharacters(input, options);
            
            Assert.Contains("inv-zerowidth", result);
            Assert.Contains("ZWSP", result);
            Assert.Contains("U+200B", result);
        }

        [Fact]
        public void VisualizeInvisibleCharacters_BiDi_ShowsMarker()
        {
            var input = "Text\u200EWithLRM";
            var options = new VisualizationOptions { ShowInvisibleCharacters = true };
            var result = _visualizer.VisualizeInvisibleCharacters(input, options);
            
            Assert.Contains("inv-bidi", result);
            Assert.Contains("LRM", result);
            Assert.Contains("U+200E", result);
        }

        [Fact]
        public void VisualizeInvisibleCharacters_Confusable_ShowsMarker()
        {
            var input = "Text\u201CWithSmartQuote";
            var options = new VisualizationOptions { ShowInvisibleCharacters = true };
            var result = _visualizer.VisualizeInvisibleCharacters(input, options);
            
            Assert.Contains("inv-confusable", result);
            Assert.Contains("≈", result);
            Assert.Contains("U+201C", result);
        }

        [Fact]
        public void VisualizeInvisibleCharacters_HighlightModeByCategory_UsesCategoryClasses()
        {
            var input = "\u0001\u200B"; // Control + ZWSP
            var options = new VisualizationOptions 
            { 
                ShowInvisibleCharacters = true, 
                HighlightMode = HighlightMode.ByCategory 
            };
            var result = _visualizer.VisualizeInvisibleCharacters(input, options);
            
            Assert.Contains("inv-control", result);
            Assert.Contains("inv-zerowidth", result);
        }

        [Fact]
        public void VisualizeInvisibleCharacters_HighlightModeAllSame_UsesGenericClass()
        {
            var input = "\u0001\u200B"; // Control + ZWSP
            var options = new VisualizationOptions 
            { 
                ShowInvisibleCharacters = true, 
                HighlightMode = HighlightMode.AllSame 
            };
            var result = _visualizer.VisualizeInvisibleCharacters(input, options);
            
            Assert.Contains("inv-all", result);
            Assert.DoesNotContain("inv-control", result);
            Assert.DoesNotContain("inv-zerowidth", result);
        }

        [Fact]
        public void VisualizeInvisibleCharacters_SkipCodeBlocks_IgnoresInCode()
        {
            var input = "Text `with\u200Bcode` and\u200Cmore.";
            var options = new VisualizationOptions 
            { 
                ShowInvisibleCharacters = true, 
                SkipCodeBlocks = true 
            };
            var result = _visualizer.VisualizeInvisibleCharacters(input, options);
            
            // Should not visualize invisible chars inside backticks
            Assert.Contains("with\u200Bcode", result); // Original char preserved in code
            Assert.Contains("ZWNJ", result); // But outside code should be visualized
        }

        [Fact]
        public void VisualizeInvisibleCharacters_FencedCodeBlock_IgnoresInCode()
        {
            var input = "Text\n```\ncode\u200Bblock\n```\nwith\u200Cinvisible";
            var options = new VisualizationOptions 
            { 
                ShowInvisibleCharacters = true, 
                SkipCodeBlocks = true 
            };
            var result = _visualizer.VisualizeInvisibleCharacters(input, options);
            
            // Should preserve invisible char in fenced code
            Assert.Contains("code\u200Bblock", result);
            // But visualize outside code
            Assert.Contains("ZWNJ", result);
        }

        [Fact]
        public void VisualizeInvisibleCharacters_MixedCharacters_VisualizesAll()
        {
            var input = "\u0001Text\u00A0with\u200B\u200Emixed\u00ADchars";
            var options = new VisualizationOptions { ShowInvisibleCharacters = true };
            var result = _visualizer.VisualizeInvisibleCharacters(input, options);
            
            // Should contain visualizations for all invisible characters
            Assert.Contains("CTRL", result);   // Control char
            Assert.Contains("⍽", result);      // NBSP
            Assert.Contains("ZWSP", result);   // Zero-width space
            Assert.Contains("LRM", result);    // BiDi control
            Assert.Contains("¬", result);      // Soft hyphen
        }

        [Fact]
        public void VisualizeInvisibleCharacters_HtmlSpecialChars_EscapesCorrectly()
        {
            var input = "<script>\u200B</script>";
            var options = new VisualizationOptions { ShowInvisibleCharacters = true };
            var result = _visualizer.VisualizeInvisibleCharacters(input, options);
            
            // Should escape HTML and visualize invisible char
            Assert.Contains("&lt;script&gt;", result);
            Assert.Contains("&lt;/script&gt;", result);
            Assert.Contains("ZWSP", result);
        }

        [Fact]
        public void VisualizeInvisibleCharacters_TooltipContent_ContainsCorrectInfo()
        {
            var input = "\u200B";
            var options = new VisualizationOptions { ShowInvisibleCharacters = true };
            var result = _visualizer.VisualizeInvisibleCharacters(input, options);
            
            Assert.Contains("title=", result);
            Assert.Contains("U+200B", result);
            Assert.Contains("remove", result); // replacement hint
        }

        [Fact]
        public void GenerateVisualizationCSS_ReturnsValidCSS()
        {
            var css = _visualizer.GenerateVisualizationCSS();
            
            Assert.Contains(".inv-char", css);
            Assert.Contains(".inv-control", css);
            Assert.Contains(".inv-zerowidth", css);
            Assert.Contains("background-color", css);
            Assert.Contains(":hover", css);
        }

        [Fact]
        public void VisualizeInvisibleCharacters_VariationSelector_ShowsCorrectMarker()
        {
            var input = "Text\uFE00WithVS1";
            var options = new VisualizationOptions { ShowInvisibleCharacters = true };
            var result = _visualizer.VisualizeInvisibleCharacters(input, options);
            
            Assert.Contains("VS1", result);
            Assert.Contains("U+FE00", result);
            Assert.Contains("inv-varsel", result);
        }

        [Fact]
        public void VisualizeInvisibleCharacters_CombiningMark_ShowsMarker()
        {
            var input = "Text\u0301WithCombining";
            var options = new VisualizationOptions { ShowInvisibleCharacters = true };
            var result = _visualizer.VisualizeInvisibleCharacters(input, options);
            
            Assert.Contains("◌", result);
            Assert.Contains("inv-combining", result);
        }
    }
}