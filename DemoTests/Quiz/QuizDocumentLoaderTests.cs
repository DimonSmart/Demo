using System.Text;
using System.Text.Json;
using Demo.Core.Quiz;

namespace DemoTests.Quiz;

public sealed class QuizDocumentLoaderTests
{
    private readonly QuizDocumentLoader loader = new();

    [Fact]
    public async Task NewCamelCaseJsonLoads()
    {
        var document = await LoadAsync(ValidJson());

        Assert.Equal("contract-fixture", document.Id);
        Assert.Equal("ru", Assert.Single(document.Languages));
    }

    [Fact]
    public async Task UnicodeIdentifiersAreAccepted()
    {
        var json = ValidJson()
            .Replace("contract-fixture", "тест 🐒", StringComparison.Ordinal)
            .Replace("topic-001-ticket-001", "билет обезьяна 🐒", StringComparison.Ordinal)
            .Replace("topic-001", "тема внутреннего диалога", StringComparison.Ordinal)
            .Replace("\"a1\"", "\"верный ответ\"", StringComparison.Ordinal)
            .Replace("\"a2\"", "\"неверный ответ\"", StringComparison.Ordinal);

        var document = await LoadAsync(json);

        Assert.Equal("тест 🐒", document.Id);
        Assert.Equal("тема внутреннего диалога", document.Topics[0].Id);
        Assert.Equal("тема внутреннего диалога", document.Questions[0].TopicId);
        Assert.Equal("билет обезьяна 🐒", document.Questions[0].Id);
    }

    [Theory]
    [InlineData("\u0001")]
    [InlineData("\u200B")]
    public async Task InvisibleOrControlCharactersInIdentifiersAreRejected(string forbiddenCharacter)
    {
        var escapedCharacter = JsonSerializer.Serialize(forbiddenCharacter)[1..^1];
        var json = ValidJson().Replace("topic-001", $"topic{escapedCharacter}001", StringComparison.Ordinal);

        await Assert.ThrowsAsync<InvalidDataException>(() => LoadAsync(json));
    }

    [Fact]
    public async Task PascalCaseNewJsonIsRejected()
    {
        var json = ValidJson().Replace("schemaVersion", "SchemaVersion", StringComparison.Ordinal);

        await Assert.ThrowsAsync<InvalidDataException>(() => LoadAsync(json));
    }

    [Fact]
    public async Task UnknownNewJsonFieldIsRejected()
    {
        var json = ValidJson().Replace("\"questions\"", "\"unknown\": true, \"questions\"", StringComparison.Ordinal);

        await Assert.ThrowsAsync<JsonException>(() => LoadAsync(json));
    }

    [Fact]
    public async Task LegacyPddJsonConvertsToUniversalDocument()
    {
        const string json = """
        {
          "Topics": [{ "Id": 7, "Slug": "tema", "Title": { "R": "Тема", "S": "Tema", "E": "Topic" } }],
          "Questions": [{
            "Id": 42,
            "TopicId": 7,
            "Q": { "R": "Вопрос", "S": "Pregunta", "E": "Question" },
            "A": [
              { "T": { "R": "Да", "S": "Sí", "E": "Yes" }, "Y": true },
              { "T": { "R": "Нет", "S": "No", "E": "No" }, "Y": false }
            ],
            "Rule": { "R": "Правило", "S": "Regla", "E": "Rule" },
            "Img": true,
            "Terms": [{ "R": "термин", "S": "término", "E": "term" }]
          }]
        }
        """;

        var document = await LoadAsync(json, new QuizLegacyLoadOptions
        {
            DocumentId = "dgt-pdd",
            ImagesBaseUrl = "https://example.com/images/",
            Title = Text("Тест", "Test", "Test")
        });

        Assert.Equal("dgt-pdd", document.Id);
        Assert.Equal(["ru", "es", "en"], document.Languages);
        Assert.Equal("42", Assert.Single(document.Questions).Id);
        Assert.Equal("42.jpg", document.Questions[0].Image);
        Assert.True(document.Questions[0].Answers[0].IsCorrect);
    }

    [Theory]
    [InlineData("ru")]
    [InlineData("en-US")]
    [InlineData("zh-Hans")]
    [InlineData("zh-Hans-CN")]
    [InlineData("de-CH-1901")]
    [InlineData("sl-rozaj")]
    [InlineData("en-US-u-hc-h12")]
    [InlineData("x-private")]
    public void ValidBcp47TagsAreAccepted(string language)
    {
        Assert.True(Bcp47LanguageTag.IsValidCanonical(language));
    }

    [Theory]
    [InlineData("en_US")]
    [InlineData("en-")]
    [InlineData("-en")]
    [InlineData("123")]
    [InlineData("русский")]
    public void InvalidBcp47TagsAreRejected(string language)
    {
        Assert.False(Bcp47LanguageTag.IsValidCanonical(language));
    }

    [Fact]
    public async Task ImageResolverUsesValidatedDocumentRoot()
    {
        var document = await LoadAsync(ValidJson(image: "q1.jpg", imagesBaseUrl: "https://example.com/images/"));

        var uri = QuestionImageResolver.ResolveQuestionImageUri(document, document.Questions[0]);

        Assert.Equal("https://example.com/images/q1.jpg", uri!.AbsoluteUri);
    }

    [Fact]
    public async Task AbsoluteImagePathIsRejected()
    {
        await Assert.ThrowsAsync<InvalidDataException>(() => LoadAsync(ValidJson(image: "https://evil.test/q1.jpg", imagesBaseUrl: "https://example.com/images/")));
    }

    private Task<QuizDocument> LoadAsync(string json, QuizLegacyLoadOptions? options = null)
    {
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        return loader.LoadAsync(stream, options, CancellationToken.None);
    }

    private static string ValidJson(string? image = null, string? imagesBaseUrl = null)
    {
        var imageProperty = image is null ? "" : $"""
            "image": "{image}",
        """;
        var imagesBaseUrlProperty = imagesBaseUrl is null ? "" : $"""
          "imagesBaseUrl": "{imagesBaseUrl}",
        """;

        return $$"""
        {
          "schemaVersion": "1.0",
          "id": "contract-fixture",
          "title": { "ru": "Тест" },
          "languages": [ "ru" ],
        {{imagesBaseUrlProperty}}
          "topics": [
            { "id": "topic-001", "title": { "ru": "Тема" } }
          ],
          "questions": [
            {
              "id": "topic-001-ticket-001",
              "type": "singleChoice",
              "topicId": "topic-001",
              "text": { "ru": "Вопрос?" },
        {{imageProperty}}
              "answers": [
                { "id": "a1", "text": { "ru": "Да" }, "isCorrect": true },
                { "id": "a2", "text": { "ru": "Нет" }, "isCorrect": false }
              ],
              "sourceReferences": [
                { "pointer": "d0001:1", "quote": { "ru": "Цитата" } }
              ]
            }
          ]
        }
        """;
    }

    private static LocalizedText Text(string ru, string es, string en) =>
        new()
        {
            Values = new Dictionary<string, string>
            {
                ["ru"] = ru,
                ["es"] = es,
                ["en"] = en
            }
        };
}
