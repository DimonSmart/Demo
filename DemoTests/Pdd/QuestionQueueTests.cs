using Demo.Demos.Pdd;

namespace DemoTests.Pdd;
public class QuestionQueueTests
{
    [Fact]
    public void ProcessAnswer_Correct_ThreeTimes_MarksLearned()
    {
        var cards = new List<QuestionStudyCard>
            {
                new() { Id = 1 }
            };
        var queue = new QuestionQueue(cards);

        var card = queue.GetNextQuestion();
        Assert.NotNull(card);

        queue.ProcessAnswer(card, true);
        queue.ProcessAnswer(card, true);
        queue.ProcessAnswer(card, true);

        Assert.Null(queue.GetNextQuestion());
    }

    [Fact]
    public void ProcessAnswer_Incorrect_ResetsCounter()
    {
        var cards = new List<QuestionStudyCard>
            {
                new() { Id = 1 }
            };
        var queue = new QuestionQueue(cards);

        var card = queue.GetNextQuestion();
        queue.ProcessAnswer(card!, true);
        Assert.Equal(1, card!.ConsecutiveCorrectCount);

        queue.ProcessAnswer(card, false);
        Assert.Equal(0, card.ConsecutiveCorrectCount);
        Assert.False(card.IsLearned);
    }

    [Fact]
    public void GetNextQuestion_ReturnsNotLearned()
    {
        var cards = new List<QuestionStudyCard>
            {
                new() { Id = 1 },
                new() { Id = 2 },
                new() { Id = 3 }
            };
        var queue = new QuestionQueue(cards);

        var c1 = queue.GetNextQuestion()!;
        queue.ProcessAnswer(c1, true);
        queue.ProcessAnswer(c1, true);
        queue.ProcessAnswer(c1, true);

        var c2 = queue.GetNextQuestion()!;
        queue.ProcessAnswer(c2, true);
        queue.ProcessAnswer(c2, true);
        queue.ProcessAnswer(c2, true);

        var c3 = queue.GetNextQuestion()!;
        Assert.Equal(3, c3.Id);
        queue.ProcessAnswer(c3, true);
        queue.ProcessAnswer(c3, true);
        queue.ProcessAnswer(c3, true);

        Assert.Null(queue.GetNextQuestion());
    }
}
