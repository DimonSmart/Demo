namespace Demo.Core.Quiz;

public interface IQuizDocumentLoader
{
    Task<QuizDocument> LoadAsync(
        Stream json,
        QuizLegacyLoadOptions? legacyOptions,
        CancellationToken cancellationToken = default);
}
