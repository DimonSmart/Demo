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

        var card = queue.PeekNextQuestionsForStudy(1).FirstOrDefault();
        Assert.NotNull(card);

        queue.ProcessAnswer(card, true);
        queue.ProcessAnswer(card, true);
        queue.ProcessAnswer(card, true);

        Assert.Empty(queue.PeekNextQuestionsForStudy(1));
    }

    [Fact]
    public void ProcessAnswer_Incorrect_ResetsCounter()
    {
        var cards = new List<QuestionStudyCard>
            {
                new() { Id = 1 }
            };
        var queue = new QuestionQueue(cards);

        var card = queue.PeekNextQuestionsForStudy(1).FirstOrDefault();
        queue.ProcessAnswer(card!, true);
        Assert.Equal(1, card!.ConsecutiveCorrectCount);

        queue.ProcessAnswer(card, false);
        Assert.Equal(0, card.ConsecutiveCorrectCount);
        Assert.False(card.IsLearned);
    }

    [Fact]
    public void PeekNextQuestionsForStudy_ReturnsNotLearnedWithoutRemoving()
    {
        var cards = new List<QuestionStudyCard>
            {
                new() { Id = 1 },
                new() { Id = 2 },
                new() { Id = 3 }
            };
        var queue = new QuestionQueue(cards);

        // Peek at first question and verify it's still in queue
        var firstPeek = queue.PeekNextQuestionsForStudy(1).First();
        Assert.Equal(3, queue.Cards.Count); // Still all cards in queue
        
        // Second peek should return same question since it wasn't processed yet
        var secondPeek = queue.PeekNextQuestionsForStudy(1).First();
        Assert.Equal(firstPeek.Id, secondPeek.Id);
        
        // Process answer to move first question back in queue
        queue.ProcessAnswer(firstPeek, true);
        
        // Next peek should now return a different question
        var thirdPeek = queue.PeekNextQuestionsForStudy(1).First();
        Assert.NotEqual(firstPeek.Id, thirdPeek.Id);
        Assert.Equal(3, queue.Cards.Count); // All cards still in queue
    }

    [Fact]
    public void ProcessAnswer_CorrectAnswer_MovesQuestionBackInQueue()
    {
        var cards = new List<QuestionStudyCard>
            {
                new() { Id = 1 },
                new() { Id = 2 },
                new() { Id = 3 }
            };
        var queue = new QuestionQueue(cards);

        var firstQuestion = queue.PeekNextQuestionsForStudy(1).First();
        queue.ProcessAnswer(firstQuestion, true);

        // After correct answer, the question should be moved back
        var nextBatch = queue.PeekNextQuestionsForStudy(3);
        Assert.Equal(3, nextBatch.Count);
        Assert.NotEqual(firstQuestion.Id, nextBatch.First().Id);
        Assert.Contains(nextBatch, q => q.Id == firstQuestion.Id); // Question still in queue
    }

    [Fact]
    public void GetNextQuestions_BatchSize_ReturnsSameQuestionsUntilProcessed()
    {
        var cards = new List<QuestionStudyCard>
        {
            new() { Id = 1 },
            new() { Id = 2 },
            new() { Id = 3 },
            new() { Id = 4 },
            new() { Id = 5 }
        };
        var queue = new QuestionQueue(cards);

        var firstBatch = queue.PeekNextQuestionsForStudy(3);
        var secondBatch = queue.PeekNextQuestionsForStudy(3);

        // Without processing answers, subsequent peeks should return same questions
        Assert.Equal(firstBatch.Select(q => q.Id), secondBatch.Select(q => q.Id));
        
        // Process first question
        queue.ProcessAnswer(firstBatch[0], true);
        
        var thirdBatch = queue.PeekNextQuestionsForStudy(3);
        // After processing first question:
        // - First question should now be different (old first question moved back in queue)
        Assert.NotEqual(firstBatch[0].Id, thirdBatch[0].Id);
        // - Second question from first batch should now be first
        Assert.Equal(firstBatch[1].Id, thirdBatch[0].Id);
        // - Third question from first batch should now be second
        Assert.Equal(firstBatch[2].Id, thirdBatch[1].Id);
        // - Original first question should not be in first 2 positions
        Assert.DoesNotContain(thirdBatch.Take(2), q => q.Id == firstBatch[0].Id);
    }
}
