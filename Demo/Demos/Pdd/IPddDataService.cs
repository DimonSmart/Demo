namespace Demo.Demos.Pdd;

public interface IPddDataService
{
    Task<Demo.Core.Quiz.QuizDocument> LoadDatabaseAsync();
}
