using System.Net;

namespace DimonSmart.StringDiff
{
    public class HighlightedStringReconstructor : StringReconstructor
    {
        private const string InsertedTextClass = "diffviewbox-inserted-text";
        private const string DeletedTextClass = "diffviewbox-deleted-text";
        private const string ModifiedTextClass = "diffviewbox-modified-text";
        private const string UnchangedTextClass = "diffviewbox-unchanged-text";

        protected override string FormatInsertedText(string text) =>
            $"<span class='{InsertedTextClass}' >{EscapeAndFormatText(text)}</span>";

        protected override string FormatDeletedText(string text) =>
            $"<span class='{DeletedTextClass}'>{EscapeAndFormatText(text)}</span>";

        protected override string FormatModifiedText(string oldText, string newText) =>
            $"<span class='{DeletedTextClass}'>{EscapeAndFormatText(oldText)}</span><span class='{ModifiedTextClass}'>{EscapeAndFormatText(newText)}</span>";

        protected override string FormatUnchangedText(string text) =>
            $"<span class='{UnchangedTextClass}'>{EscapeAndFormatText(text)}</span>";

        private static string EscapeAndFormatText(string text)
        {
            return string.IsNullOrEmpty(text) ? text : WebUtility.HtmlEncode(text).Replace("\n", "<br>");
        }
    }
}
