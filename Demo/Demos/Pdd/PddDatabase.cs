namespace Demo.Demos.Pdd;

public sealed class PddDatabase
{
    public const string SupportedSchemaVersion = "1.0";

    public string SchemaVersion { get; init; } = string.Empty;
    public string Id { get; init; } = string.Empty;
    public LocalizedText Title { get; init; } = new();
    public List<string> Languages { get; init; } = [];
    public string? ImagesBaseUrl { get; init; }
    public List<Topic> Topics { get; init; } = [];
    public List<QuestionItem> Questions { get; init; } = [];
}
