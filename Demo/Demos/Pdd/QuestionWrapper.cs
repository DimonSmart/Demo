namespace Demo.Demos.Pdd;

public class QuestionWrapper
{
    public QuestionItem Question { get; set; } = null!;
    public QuestionStudyCard StudyCard { get; set; } = null!;
    public bool HasAnswered { get; set; }
    public AnswerItem? SelectedAnswer { get; set; }
    public bool ShowExplanation { get; set; }
    public bool ShowSecondaryLanguage1 { get; set; } = false;
    public bool ShowSecondaryLanguage2 { get; set; } = false;
}