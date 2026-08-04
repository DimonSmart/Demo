using Demo.Core.Quiz;

namespace Demo.Demos.Quiz;

public class QuestionWrapper
{
    public QuizQuestion Question { get; set; } = null!;
    public QuestionStudyCard StudyCard { get; set; } = null!;
    public bool HasAnswered { get; set; }
    public QuizAnswer? SelectedAnswer { get; set; }
    public bool ShowExplanation { get; set; }
}
