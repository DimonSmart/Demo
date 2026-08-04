namespace Demo.Demos.Quiz;

public class QuizUserPreferences
{
    public bool HighlightTerms { get; set; }
    public string PrimaryLanguage { get; set; } = string.Empty;
    public string? LastPracticedQuizSourceId { get; set; }
}
