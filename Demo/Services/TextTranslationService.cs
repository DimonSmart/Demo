using Demo.Demos.Pdd;
using Markdig;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Demo.Services
{
    public class TextTranslationService
    {
        private readonly MarkdownPipeline _markdownPipeline;
        
        // A set of dark colors used for highlighting different terms
        private static readonly string[] TermColors = new[]
        {
            "#8B008B", // DarkMagenta
            "#800000", // Maroon
            "#004080", // Dark blue
            "#808000", // Olive
            "#400080"  // Another dark shade
        };

        public TextTranslationService()
        {
            _markdownPipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .Build();
        }

        public string GetLocalizedContent(LocalizedText text, string languageCode, List<LocalizedText>? terms = null, bool highlightTerms = false)
        {
            string content = "";

            // Get content for the requested language
            if (languageCode == "en" && !string.IsNullOrWhiteSpace(text.English))
                content = text.English;
            else if (languageCode == "es" && !string.IsNullOrWhiteSpace(text.Spanish))
                content = text.Spanish;
            else if (languageCode == "ru" && !string.IsNullOrWhiteSpace(text.Russian))
                content = text.Russian;

            if (string.IsNullOrEmpty(content))
                return string.Empty;

            // Apply term highlighting if needed
            if (highlightTerms && terms != null && terms.Any())
            {
                if (languageCode == "ru")
                    content = HighlightAllTerms(content, terms, term => term.Russian);
                else if (languageCode == "es")
                    content = HighlightAllTerms(content, terms, term => term.Spanish);
                else if (languageCode == "en")
                    content = HighlightAllTerms(content, terms, term => term.English);
            }

            // Convert markdown to HTML
            return Markdown.ToHtml(content, _markdownPipeline);
        }

        private string HighlightAllTerms(
            string originalText,
            List<LocalizedText> terms,
            Func<LocalizedText, string> selectTermText)
        {
            var result = originalText;

            for (int i = 0; i < terms.Count && i < TermColors.Length; i++)
            {
                var term = selectTermText(terms[i]);
                if (string.IsNullOrWhiteSpace(term))
                    continue;

                var pattern = $"\\b{Regex.Escape(term)}\\b";
                var color = TermColors[i];

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
