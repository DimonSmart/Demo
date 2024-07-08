using System.Text;
using System.Net;


namespace DimonSmart.StringDiff
{
    public class HighlightedStringReconstructor : StringReconstructor
    {
        private const string InsertedTextClass = "inserted-text";
        private const string DeletedTextClass = "deleted-text";
        private const string ModifiedTextClass = "modified-text";

        protected override string FormatInsertedText(string text) =>
            $"<span class='{InsertedTextClass}'>{EscapeAndFormatText(text)}</span>";

        protected override string FormatDeletedText(string text) =>
            $"<span class='{DeletedTextClass}'>{EscapeAndFormatText(text)}</span>";

        protected override string FormatModifiedText(string oldText, string newText) =>
            $"<span class='{DeletedTextClass}'>{EscapeAndFormatText(oldText)}</span><span class='{ModifiedTextClass}'>{EscapeAndFormatText(newText)}</span>";

        private string EscapeAndFormatText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            // Use WebUtility.HtmlEncode to escape HTML special characters
            var escapedText = WebUtility.HtmlEncode(text);

            // Replace newlines with <br> tags
            return escapedText.Replace("\n", "<br>");
        }
    }
}
