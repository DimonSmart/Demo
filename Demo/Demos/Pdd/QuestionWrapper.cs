namespace Demo.Demos.Pdd;

public class QuestionWrapper
{
    public QuestionItem Question { get; set; } = null!;
    public QuestionStudyCard StudyCard { get; set; } = null!;
    public bool HasAnswered { get; set; }
    // public bool IsCorrect { get; set; }
    public AnswerItem? SelectedAnswer { get; set; }
    public bool ShowExplanation { get; set; }
}