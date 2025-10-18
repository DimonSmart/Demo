using System;
using System.Text;

namespace MarkdownToWordFeature.Services
{
    public static class MarkdownProcessor
    {
        private static readonly string[] DistortionSequences =
        {
            "\u0336\u0335\u0334",
            "\u0335\u0338\u0311",
            "\u0336\u0306\u0307",
            "\u0337\u0335\u0302"
        };

        public static string ToggleMarkdown(string content, int selectionStart, int selectionEnd, string markdownSymbol)
        {
            if (selectionStart < 0 || selectionEnd <= selectionStart || selectionStart >= content.Length)
                throw new ArgumentException("Invalid selection range.");

            if (string.IsNullOrEmpty(markdownSymbol))
                throw new ArgumentException("Markdown symbol cannot be null or empty.");

            while (selectionStart < selectionEnd && char.IsWhiteSpace(content[selectionStart]))
                selectionStart++;

            while (selectionEnd > selectionStart && char.IsWhiteSpace(content[selectionEnd - 1]))
                selectionEnd--;

            var selectedText = content.Substring(selectionStart, selectionEnd - selectionStart);
            var preText = content[..selectionStart];
            var postText = content[selectionEnd..];

            var isSurrounded = selectedText.StartsWith(markdownSymbol) && selectedText.EndsWith(markdownSymbol);
            var isWordSurrounded = preText.EndsWith(markdownSymbol) && postText.StartsWith(markdownSymbol);

            if (isSurrounded)
            {
                selectedText = selectedText.Substring(markdownSymbol.Length, selectedText.Length - 2 * markdownSymbol.Length);
            }
            else if (isWordSurrounded)
            {
                preText = preText.Substring(0, preText.Length - markdownSymbol.Length);
                postText = postText.Substring(markdownSymbol.Length);
            }
            else
            {
                selectedText = markdownSymbol + selectedText + markdownSymbol;
            }

            return preText + selectedText + postText;
        }

        public static string ApplyDiacriticStrike(string content, int selectionStart, int selectionEnd)
        {
            if (content is null)
                throw new ArgumentNullException(nameof(content));

            if (selectionStart < 0 || selectionEnd <= selectionStart || selectionStart >= content.Length)
                throw new ArgumentException("Invalid selection range.");

            if (selectionEnd > content.Length)
                selectionEnd = content.Length;

            while (selectionStart < selectionEnd && char.IsWhiteSpace(content[selectionStart]))
                selectionStart++;

            while (selectionEnd > selectionStart && char.IsWhiteSpace(content[selectionEnd - 1]))
                selectionEnd--;

            if (selectionStart >= selectionEnd)
                return content;

            var selectedText = content.Substring(selectionStart, selectionEnd - selectionStart);
            var transformed = ApplyDistortion(selectedText);

            return content[..selectionStart] + transformed + content[selectionEnd..];
        }

        private static string ApplyDistortion(string text)
        {
            var builder = new StringBuilder(text.Length * 4);
            var sequenceIndex = 0;

            foreach (var character in text)
            {
                builder.Append(character);

                if (!char.IsWhiteSpace(character))
                {
                    builder.Append(DistortionSequences[sequenceIndex]);
                    sequenceIndex = (sequenceIndex + 1) % DistortionSequences.Length;
                }
            }

            return builder.ToString();
        }
    }
}
