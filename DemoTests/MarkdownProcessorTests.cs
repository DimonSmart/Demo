using Demo.Demos.MarkdownToWord;

namespace DemoTests
{
    public class MarkdownProcessorTests
    {
        [Theory]
        [InlineData("Hello world", 0, 5, "**Hello** world")]
        [InlineData("Hello world", 0, 6, "**Hello** world")]
        [InlineData("**Hello** world", 0, 9, "Hello world")]
        [InlineData("**Hello** world", 2, 7, "Hello world")]
        [InlineData("Hello **world**", 6, 15, "Hello world")]
        [InlineData("Hello world", 6, 11, "Hello **world**")]
        public void ToggleBold_WorksCorrectly(string input, int start, int end, string expected)
        {
            var result = MarkdownProcessor.ToggleMarkdown(input, start, end, "**");
            Assert.Equal(expected, result);
        }
    }
}