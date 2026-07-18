using System.Text.RegularExpressions;

namespace Demo.Demos.Pdd;

public static class QuizDocumentValidator
{
    private static readonly Regex LanguageCode = new("^[a-z]{2,3}(?:-[A-Z][a-z]{3}|-[A-Z]{2}|-[0-9]{3})*$", RegexOptions.Compiled);
    private static readonly Regex DocumentId = new("^[a-z0-9][a-z0-9._-]*$", RegexOptions.Compiled);

    public static void Validate(PddDatabase document)
    {
        if (document.SchemaVersion != PddDatabase.SupportedSchemaVersion) throw new InvalidDataException($"Unsupported quiz schema version '{document.SchemaVersion}'.");
        if (!DocumentId.IsMatch(document.Id)) throw new InvalidDataException("Quiz id must be a non-empty stable identifier.");
        if (document.Languages.Count == 0 || document.Languages.Distinct(StringComparer.Ordinal).Count() != document.Languages.Count || document.Languages.Any(language => !LanguageCode.IsMatch(language))) throw new InvalidDataException("Quiz languages must contain unique BCP 47 language codes.");
        ValidateText(document.Title, document.Languages, "Quiz title");
        if (document.Topics.Select(topic => topic.Id).Distinct(StringComparer.Ordinal).Count() != document.Topics.Count) throw new InvalidDataException("Topic ids must be unique.");
        if (document.Questions.Select(question => question.Id).Distinct(StringComparer.Ordinal).Count() != document.Questions.Count) throw new InvalidDataException("Question ids must be unique.");
        var topicIds = document.Topics.Select(topic => topic.Id).ToHashSet(StringComparer.Ordinal);
        foreach (var topic in document.Topics) { if (string.IsNullOrWhiteSpace(topic.Id)) throw new InvalidDataException("Topic id is required."); ValidateText(topic.Title, document.Languages, $"Topic '{topic.Id}' title"); }
        foreach (var question in document.Questions) ValidateQuestion(question, document, topicIds);
    }

    private static void ValidateQuestion(QuestionItem question, PddDatabase document, HashSet<string> topicIds)
    {
        if (string.IsNullOrWhiteSpace(question.Id) || !topicIds.Contains(question.TopicId)) throw new InvalidDataException($"Question '{question.Id}' has an invalid id or topicId.");
        if (question.Type != "singleChoice") throw new InvalidDataException($"Question '{question.Id}' has unsupported type '{question.Type}'.");
        ValidateText(question.Text, document.Languages, $"Question '{question.Id}' text");
        if (question.Answers.Count < 2 || question.Answers.Count(answer => answer.IsCorrect) != 1 || question.Answers.Select(answer => answer.Id).Distinct(StringComparer.Ordinal).Count() != question.Answers.Count) throw new InvalidDataException($"Question '{question.Id}' must have unique answers and exactly one correct answer.");
        foreach (var answer in question.Answers) { if (string.IsNullOrWhiteSpace(answer.Id)) throw new InvalidDataException($"Question '{question.Id}' contains an answer without id."); ValidateText(answer.Text, document.Languages, $"Answer '{answer.Id}'"); }
        if (question.Explanation is not null) ValidateText(question.Explanation, document.Languages, $"Question '{question.Id}' explanation");
        foreach (var term in question.Terms ?? []) ValidateText(term, document.Languages, $"Question '{question.Id}' term");
        if (question.Image is not null && (string.IsNullOrWhiteSpace(document.ImagesBaseUrl) || Uri.TryCreate(question.Image, UriKind.Absolute, out _) || question.Image.StartsWith('/') || question.Image.Split('/').Any(segment => segment == ".."))) throw new InvalidDataException($"Question '{question.Id}' has an invalid image path.");
    }

    private static void ValidateText(LocalizedText text, IEnumerable<string> languages, string name)
    {
        if (languages.Any(language => string.IsNullOrWhiteSpace(text.Get(language)))) throw new InvalidDataException($"{name} must contain non-empty text for every document language.");
    }
}
