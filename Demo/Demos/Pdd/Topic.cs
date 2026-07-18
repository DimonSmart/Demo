namespace Demo.Demos.Pdd;

public sealed class Topic
{
    public string Id { get; init; } = string.Empty;
    public string? Slug { get; init; }
    public LocalizedText Title { get; init; } = new();
}
