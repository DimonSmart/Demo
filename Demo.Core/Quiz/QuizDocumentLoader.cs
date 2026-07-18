using System.Globalization;
using System.Text.Json;

namespace Demo.Core.Quiz;

public sealed class QuizDocumentLoader : IQuizDocumentLoader
{
    public async Task<QuizDocument> LoadAsync(
        Stream json,
        QuizLegacyLoadOptions? legacyOptions,
        CancellationToken cancellationToken = default)
    {
        using var buffer = new MemoryStream();
        await json.CopyToAsync(buffer, cancellationToken);
        buffer.Position = 0;

        using var root = await JsonDocument.ParseAsync(buffer, cancellationToken: cancellationToken);
        var rootElement = root.RootElement;
        if (rootElement.ValueKind != JsonValueKind.Object)
            throw new InvalidDataException("Quiz document root must be an object.");

        if (rootElement.TryGetProperty("schemaVersion", out var schemaVersion))
        {
            if (schemaVersion.GetString() != QuizDocument.SupportedSchemaVersion)
                throw new InvalidDataException($"Unsupported quiz schema version '{schemaVersion.GetString()}'.");

            buffer.Position = 0;
            var document = await JsonSerializer.DeserializeAsync<QuizDocument>(
                buffer,
                QuizJson.SerializerOptions,
                cancellationToken) ?? throw new InvalidDataException("Quiz document is empty.");
            return QuizDocumentValidator.Validate(document);
        }

        if (IsLegacyPdd(rootElement))
        {
            if (legacyOptions is null)
                throw new InvalidDataException("Legacy quiz document requires legacy load options.");

            buffer.Position = 0;
            var legacyDocument = await JsonSerializer.DeserializeAsync<LegacyPddDocument>(
                buffer,
                cancellationToken: cancellationToken) ?? throw new InvalidDataException("Legacy quiz document is empty.");
            return QuizDocumentValidator.Validate(ConvertLegacy(legacyDocument, legacyOptions));
        }

        throw new InvalidDataException("Quiz document format is not recognized.");
    }

    private static bool IsLegacyPdd(JsonElement root) =>
        root.TryGetProperty("Topics", out _) && root.TryGetProperty("Questions", out _);

    private static QuizDocument ConvertLegacy(LegacyPddDocument legacyDocument, QuizLegacyLoadOptions options)
    {
        var topics = legacyDocument.Topics
            .Select(topic => new QuizTopic
            {
                Id = ToInvariantString(topic.Id),
                Slug = topic.Slug,
                Title = ToLocalizedText(topic.Title.R, topic.Title.S, topic.Title.E)
            })
            .ToArray();

        var questions = legacyDocument.Questions
            .Select(question =>
            {
                var questionId = ToInvariantString(question.Id);
                return new QuizQuestion
                {
                    Id = questionId,
                    Type = "singleChoice",
                    TopicId = ToInvariantString(question.TopicId),
                    Text = ToLocalizedText(question.Q.R, question.Q.S, question.Q.E),
                    Answers = question.A.Select((answer, index) => new QuizAnswer
                    {
                        Id = $"a{index + 1}",
                        Text = ToLocalizedText(answer.T.R, answer.T.S, answer.T.E),
                        IsCorrect = answer.Y
                    }).ToArray(),
                    Explanation = ToOptionalLocalizedText(question.Rule),
                    Image = question.Img ? $"{questionId}.jpg" : null,
                    Terms = question.Terms?.Select(term => ToLocalizedText(term.R, term.S, term.E)).ToArray()
                };
            })
            .ToArray();

        return new QuizDocument
        {
            SchemaVersion = QuizDocument.SupportedSchemaVersion,
            Id = options.DocumentId,
            Title = options.Title,
            Languages = ["ru", "es", "en"],
            ImagesBaseUrl = options.ImagesBaseUrl,
            Topics = topics,
            Questions = questions
        };
    }

    private static LocalizedText ToLocalizedText(string? ru, string? es, string? en) =>
        new()
        {
            Values = new Dictionary<string, string>
            {
                ["ru"] = ru ?? string.Empty,
                ["es"] = es ?? string.Empty,
                ["en"] = en ?? string.Empty
            }
        };

    private static LocalizedText? ToOptionalLocalizedText(LegacyPddLocalizedText? text)
    {
        if (text is null) return null;
        return string.IsNullOrWhiteSpace(text.R)
            || string.IsNullOrWhiteSpace(text.S)
            || string.IsNullOrWhiteSpace(text.E)
                ? null
                : ToLocalizedText(text.R, text.S, text.E);
    }

    private static string ToInvariantString(int value) => value.ToString(CultureInfo.InvariantCulture);

    private sealed class LegacyPddDocument
    {
        public List<LegacyPddTopic> Topics { get; init; } = [];
        public List<LegacyPddQuestion> Questions { get; init; } = [];
    }

    private sealed class LegacyPddTopic
    {
        public int Id { get; init; }
        public string? Slug { get; init; }
        public LegacyPddLocalizedText Title { get; init; } = new();
    }

    private sealed class LegacyPddQuestion
    {
        public int Id { get; init; }
        public int TopicId { get; init; }
        public LegacyPddLocalizedText Q { get; init; } = new();
        public List<LegacyPddAnswer> A { get; init; } = [];
        public LegacyPddLocalizedText? Rule { get; init; }
        public bool Img { get; init; }
        public List<LegacyPddLocalizedText>? Terms { get; init; }
    }

    private sealed class LegacyPddAnswer
    {
        public LegacyPddLocalizedText T { get; init; } = new();
        public bool Y { get; init; }
    }

    private sealed class LegacyPddLocalizedText
    {
        public string? R { get; init; }
        public string? S { get; init; }
        public string? E { get; init; }
    }
}
