using System.Security.Cryptography;
using Demo.Core.Quiz;

namespace Demo.Demos.Quiz;
public sealed class LoadedQuizDocument { public required QuizCatalogEntry Source { get; init; } public required QuizDocument Document { get; init; } public required string DocumentHash { get; init; } public required IReadOnlyDictionary<string, string> QuestionHashes { get; init; } }
public interface IQuizSourceLoader { Task<LoadedQuizDocument> LoadAsync(QuizCatalogEntry source, CancellationToken cancellationToken = default); }
public sealed class QuizSourceLoader(HttpClient httpClient, IQuizDocumentLoader documentLoader, IConfiguration configuration) : IQuizSourceLoader
{
    public async Task<LoadedQuizDocument> LoadAsync(QuizCatalogEntry source, CancellationToken cancellationToken = default)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken); timeout.CancelAfter(TimeSpan.FromSeconds(30));
        using var response = await httpClient.GetAsync(source.Url, HttpCompletionOption.ResponseHeadersRead, timeout.Token);
        response.EnsureSuccessStatusCode();
        var max = configuration.GetValue<long?>("Quiz:MaxDocumentBytes") ?? 20 * 1024 * 1024;
        if (response.Content.Headers.ContentLength > max) throw new InvalidDataException("Quiz document exceeds the allowed size.");
        await using var input = await response.Content.ReadAsStreamAsync(timeout.Token); using var buffer = new MemoryStream();
        await input.CopyToAsync(buffer, timeout.Token); if (buffer.Length > max) throw new InvalidDataException("Quiz document exceeds the allowed size.");
        var bytes = buffer.ToArray(); buffer.Position = 0;
        var legacy = source.IsBuiltIn ? CreateLegacyOptions() : null;
        var document = await documentLoader.LoadAsync(buffer, legacy, timeout.Token);
        return new LoadedQuizDocument { Source = source, Document = document, DocumentHash = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant(), QuestionHashes = document.Questions.ToDictionary(q => q.Id, QuestionContentHasher.Hash, StringComparer.Ordinal) };
    }
    private QuizLegacyLoadOptions CreateLegacyOptions() => new() { DocumentId = configuration["Quiz:LegacyDocumentId"] ?? throw new InvalidOperationException("Quiz:LegacyDocumentId is not configured."), ImagesBaseUrl = configuration["Quiz:LegacyImagesBaseUrl"], Title = new LocalizedText { Values = configuration.GetSection("Quiz:LegacyTitle").Get<Dictionary<string, string>>() ?? throw new InvalidOperationException("Quiz:LegacyTitle is not configured.") } };
}
