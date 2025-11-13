using Demo.Demos.Pdd;
using Markdig;
using System.Text.RegularExpressions;

namespace Demo.Services
{
    public class TextTranslationService
    {
        private readonly MarkdownPipeline _markdownPipeline;

        private static readonly string[] HighlightColors =
        {
            "#8B008B",
            "#800000",
            "#004080",
            "#808000",
            "#400080"
        };

        public TextTranslationService()
        {
            _markdownPipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .Build();
        }

        public string GetLocalizedContent(LocalizedText text, string languageCode, List<LocalizedText>? terms = null, bool highlightTerms = false)
        {
            var content = languageCode switch
            {
                "en" when !string.IsNullOrWhiteSpace(text.English) => text.English!,
                "es" when !string.IsNullOrWhiteSpace(text.Spanish) => text.Spanish!,
                "ru" when !string.IsNullOrWhiteSpace(text.Russian) => text.Russian!,
                _ => string.Empty
            };

            if (string.IsNullOrEmpty(content))
            {
                return string.Empty;
            }

            if (highlightTerms && terms is { Count: > 0 })
            {
                content = languageCode switch
                {
                    "ru" => HighlightAllTerms(content, terms, term => term.Russian),
                    "es" => HighlightAllTerms(content, terms, term => term.Spanish),
                    "en" => HighlightAllTerms(content, terms, term => term.English),
                    _ => content
                };
            }

            return Markdown.ToHtml(content, _markdownPipeline);
        }

        private string HighlightAllTerms(
            string originalText,
            List<LocalizedText> terms,
            Func<LocalizedText, string> selectTermText)
        {
            var result = originalText;

            for (var i = 0; i < terms.Count && i < HighlightColors.Length; i++)
            {
                var term = selectTermText(terms[i]);
                if (string.IsNullOrWhiteSpace(term))
                    continue;

                var pattern = $"\\b{Regex.Escape(term)}\\b";
                var color = HighlightColors[i];

                result = Regex.Replace(
                    result,
                    pattern,
                    match => $"<span style=\"font-weight:bold; color:{color}\">{match.Value}</span>",
                    RegexOptions.IgnoreCase
                );
            }

            return result;
        }
    }
}
