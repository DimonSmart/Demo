namespace Demo.Core.Quiz;

public static class QuestionImageResolver
{
    public static Uri? ResolveQuestionImageUri(QuizDocument document, QuizQuestion question)
    {
        if (string.IsNullOrWhiteSpace(question.Image)) return null;
        return new Uri(new Uri(document.ImagesBaseUrl!, UriKind.Absolute), question.Image);
    }
}
