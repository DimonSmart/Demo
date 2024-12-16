namespace Demo.Demos.MarkdownToWord
{
    public static class MarkdownProcessor
    {
        public static string ToggleMarkdown(string content, int selectionStart, int selectionEnd, string markdownSymbol)
        {
            if (selectionStart < 0 || selectionEnd <= selectionStart || selectionStart >= content.Length)
                throw new ArgumentException("Invalid selection range.");

            if (string.IsNullOrEmpty(markdownSymbol))
                throw new ArgumentException("Markdown symbol cannot be null or empty.");

            // Adjust selection range to ignore surrounding spaces
            while (selectionStart < selectionEnd && char.IsWhiteSpace(content[selectionStart]))
                selectionStart++;

            while (selectionEnd > selectionStart && char.IsWhiteSpace(content[selectionEnd - 1]))
                selectionEnd--;

            var selectedText = content.Substring(selectionStart, selectionEnd - selectionStart);
            var preText = content[..selectionStart];
            var postText = content[selectionEnd..];

            // Check if selection or word is already surrounded by the markdown symbol
            var isSurrounded = selectedText.StartsWith(markdownSymbol) && selectedText.EndsWith(markdownSymbol);
            var isWordSurrounded = preText.EndsWith(markdownSymbol) && postText.StartsWith(markdownSymbol);

            if (isSurrounded)
            {
                // Remove markdown symbol from selection
                selectedText = selectedText.Substring(markdownSymbol.Length, selectedText.Length - 2 * markdownSymbol.Length);
            }
            else if (isWordSurrounded)
            {
                // Remove markdown symbol from the surrounding word
                preText = preText.Substring(0, preText.Length - markdownSymbol.Length);
                postText = postText.Substring(markdownSymbol.Length);
            }
            else
            {
                // Add markdown symbol to the selection
                selectedText = markdownSymbol + selectedText + markdownSymbol;
            }

            return preText + selectedText + postText;
        }

    }
}
