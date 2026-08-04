using System.Security.Cryptography;
using System.Text;
using Demo.Core.Quiz;

namespace Demo.Demos.Quiz;
public static class QuestionContentHasher
{
    public static string Hash(QuizQuestion question)
    {
        static string Text(string value) => string.Join(' ', value.Normalize(NormalizationForm.FormC).Replace("\r\n", "\n").Replace('\r', '\n').Trim().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        string Local(LocalizedText text) => string.Join("|", text.Values.OrderBy(x => x.Key, StringComparer.Ordinal).Select(x => $"{Text(x.Key)}={Text(x.Value)}"));
        var answers = question.Answers.Select(a => $"{Local(a.Text)}:{a.IsCorrect}").OrderBy(x => x, StringComparer.Ordinal);
        var canonical = $"{Text(question.Type)}\n{Local(question.Text)}\n{string.Join("\n", answers)}\n{Text(question.Image ?? string.Empty)}";
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical))).ToLowerInvariant();
    }
}
