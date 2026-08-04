namespace Demo.Demos.Quiz;

public interface IQuizDataService
{
    Task<Demo.Core.Quiz.QuizDocument> LoadDatabaseAsync();
}
