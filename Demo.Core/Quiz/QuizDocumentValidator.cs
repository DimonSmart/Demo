using System.Buffers;
using System.Globalization;
using System.Text;

namespace Demo.Core.Quiz;

public static partial class QuizDocumentValidator
{
    public static QuizDocument Validate(QuizDocument document)
    {
        if (document.SchemaVersion != QuizDocument.SupportedSchemaVersion)
            throw new InvalidDataException($"Unsupported quiz schema version '{document.SchemaVersion}'.");
        if (!IsValidId(document.Id))
            throw new InvalidDataException("Quiz document id is invalid.");
        if (document.Languages.Count == 0)
            throw new InvalidDataException("Quiz document must contain at least one language.");

        var languages = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var language in document.Languages)
        {
            if (!Bcp47LanguageTag.IsValidCanonical(language))
                throw new InvalidDataException($"Language tag '{language}' is invalid or not canonical.");
            if (!languages.Add(language))
                throw new InvalidDataException($"Language tag '{language}' is duplicated.");
        }

        ValidateRequiredText(document.Title, document.Languages, "document title");
        var normalizedImagesBaseUrl = ValidateImagesBaseUrl(document.ImagesBaseUrl);

        var topicIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var topic in document.Topics)
        {
            if (!IsValidId(topic.Id))
                throw new InvalidDataException($"Topic id '{topic.Id}' is invalid.");
            if (!topicIds.Add(topic.Id))
                throw new InvalidDataException($"Topic id '{topic.Id}' is duplicated.");
            ValidateRequiredText(topic.Title, document.Languages, $"topic '{topic.Id}' title");
        }

        var questionIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var question in document.Questions)
        {
            ValidateQuestion(question, document, topicIds, questionIds, normalizedImagesBaseUrl);
        }

        return document.ImagesBaseUrl == normalizedImagesBaseUrl
            ? document
            : new QuizDocument
            {
                SchemaVersion = document.SchemaVersion,
                Id = document.Id,
                Title = document.Title,
                Languages = document.Languages,
                ImagesBaseUrl = normalizedImagesBaseUrl,
                Topics = document.Topics,
                Questions = document.Questions
            };
    }

    private static void ValidateQuestion(
        QuizQuestion question,
        QuizDocument document,
        HashSet<string> topicIds,
        HashSet<string> questionIds,
        string? imagesBaseUrl)
    {
        if (!IsValidId(question.Id))
            throw new InvalidDataException($"Question id '{question.Id}' is invalid.");
        if (!questionIds.Add(question.Id))
            throw new InvalidDataException($"Question id '{question.Id}' is duplicated.");
        if (!topicIds.Contains(question.TopicId))
            throw new InvalidDataException($"Question '{question.Id}' references unknown topic '{question.TopicId}'.");
        if (!string.Equals(question.Type, "singleChoice", StringComparison.Ordinal))
            throw new InvalidDataException($"Question '{question.Id}' has unsupported type '{question.Type}'.");

        ValidateRequiredText(question.Text, document.Languages, $"question '{question.Id}' text");
        ValidateOptionalText(question.Explanation, document.Languages, $"question '{question.Id}' explanation");
        ValidateTerms(question, document.Languages);
        ValidateSourceReferences(question, document.Languages);
        ValidateAnswers(question, document.Languages);
        ValidateQuestionImage(question, imagesBaseUrl);
    }

    private static void ValidateAnswers(QuizQuestion question, IReadOnlyList<string> languages)
    {
        if (question.Answers.Count < 2)
            throw new InvalidDataException($"Question '{question.Id}' must contain at least two answers.");
        if (question.Answers.Count(answer => answer.IsCorrect) != 1)
            throw new InvalidDataException($"Question '{question.Id}' must contain exactly one correct answer.");

        var answerIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var answer in question.Answers)
        {
            if (!IsValidId(answer.Id))
                throw new InvalidDataException($"Question '{question.Id}' contains invalid answer id '{answer.Id}'.");
            if (!answerIds.Add(answer.Id))
                throw new InvalidDataException($"Question '{question.Id}' contains duplicated answer id '{answer.Id}'.");
            ValidateRequiredText(answer.Text, languages, $"answer '{answer.Id}' text");
        }
    }

    private static void ValidateTerms(QuizQuestion question, IReadOnlyList<string> languages)
    {
        if (question.Terms is null) return;
        foreach (var term in question.Terms)
        {
            if (term is null)
                throw new InvalidDataException($"Question '{question.Id}' contains null term.");
            ValidateRequiredText(term, languages, $"question '{question.Id}' term");
        }
    }

    private static void ValidateSourceReferences(QuizQuestion question, IReadOnlyList<string> languages)
    {
        if (question.SourceReferences is null) return;
        foreach (var sourceReference in question.SourceReferences)
        {
            if (sourceReference is null)
                throw new InvalidDataException($"Question '{question.Id}' contains null source reference.");
            if (string.IsNullOrWhiteSpace(sourceReference.Pointer))
                throw new InvalidDataException($"Question '{question.Id}' contains source reference with empty pointer.");
            ValidateOptionalText(sourceReference.Quote, languages, $"source reference '{sourceReference.Pointer}' quote");
        }
    }

    private static string? ValidateImagesBaseUrl(string? imagesBaseUrl)
    {
        if (string.IsNullOrWhiteSpace(imagesBaseUrl)) return null;
        if (imagesBaseUrl.StartsWith("//", StringComparison.Ordinal))
            throw new InvalidDataException("imagesBaseUrl must not be protocol-relative.");
        if (!Uri.TryCreate(imagesBaseUrl, UriKind.Absolute, out var uri))
            throw new InvalidDataException("imagesBaseUrl must be an absolute URI.");
        if (uri.Scheme is not "https" and not "http")
            throw new InvalidDataException("imagesBaseUrl must use http or https.");

        var normalized = uri.AbsoluteUri.EndsWith("/", StringComparison.Ordinal) ? uri.AbsoluteUri : uri.AbsoluteUri + "/";
        return normalized;
    }

    private static void ValidateQuestionImage(QuizQuestion question, string? imagesBaseUrl)
    {
        if (string.IsNullOrWhiteSpace(question.Image)) return;
        if (string.IsNullOrWhiteSpace(imagesBaseUrl))
            throw new InvalidDataException($"Question '{question.Id}' has image, but imagesBaseUrl is not configured.");
        if (question.Image.Contains('\\', StringComparison.Ordinal))
            throw new InvalidDataException($"Question '{question.Id}' image must not contain backslashes.");
        if (question.Image.StartsWith("/", StringComparison.Ordinal) || question.Image.StartsWith("\\", StringComparison.Ordinal))
            throw new InvalidDataException($"Question '{question.Id}' image must be relative.");
        if (Uri.TryCreate(question.Image, UriKind.Absolute, out _))
            throw new InvalidDataException($"Question '{question.Id}' image must not be an absolute URI.");
        if (question.Image.Split('/').Any(segment => segment == ".."))
            throw new InvalidDataException($"Question '{question.Id}' image must not contain '..' segments.");

        var baseUri = new Uri(imagesBaseUrl, UriKind.Absolute);
        var resolved = new Uri(baseUri, question.Image);
        if (!resolved.AbsoluteUri.StartsWith(baseUri.AbsoluteUri, StringComparison.Ordinal))
            throw new InvalidDataException($"Question '{question.Id}' image resolves outside imagesBaseUrl.");
    }

    private static void ValidateRequiredText(LocalizedText text, IReadOnlyList<string> languages, string name)
    {
        foreach (var language in languages)
        {
            if (!text.Values.TryGetValue(language, out var value) || string.IsNullOrWhiteSpace(value))
                throw new InvalidDataException($"Missing '{language}' localization for {name}.");
        }
    }

    private static void ValidateOptionalText(LocalizedText? text, IReadOnlyList<string> languages, string name)
    {
        if (text is null) return;
        ValidateRequiredText(text, languages, name);
    }

    private static bool IsValidId(string id)
    {
        if (id.Length == 0) return false;

        var remaining = id.AsSpan();
        while (!remaining.IsEmpty)
        {
            var status = Rune.DecodeFromUtf16(remaining, out var rune, out var charsConsumed);
            if (status != OperationStatus.Done) return false;
            if (Rune.GetUnicodeCategory(rune) is UnicodeCategory.Control or UnicodeCategory.Format) return false;
            remaining = remaining[charsConsumed..];
        }

        return true;
    }
}
