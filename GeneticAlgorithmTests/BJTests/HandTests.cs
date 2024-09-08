using Demo.Demos.BJ;

namespace GeneticAlgorithmTests.BJTests
{
    public class HandTests
    {
        [Theory]
        [InlineData(new Rank[] { }, new[] { Rank.Ace }, 11)]
        [InlineData(new Rank[] { }, new[] { Rank.Ten }, 10)]
        [InlineData(new Rank[] { }, new[] { Rank.Ace, Rank.Ace }, 12)]
        [InlineData(new Rank[] { }, new[] { Rank.Ten, Rank.Ace }, 21)]
        [InlineData(new Rank[] { }, new[] { Rank.Ten, Rank.Ten }, 20)]
        [InlineData(new Rank[] { }, new[] { Rank.Ace, Rank.Ace, Rank.Ace }, 13)]
        [InlineData(new Rank[] { }, new[] { Rank.Ten, Rank.Ace, Rank.Ace }, 12)]
        [InlineData(new Rank[] { }, new[] { Rank.Ten, Rank.Ten, Rank.Ace }, 21)]
        [InlineData(new Rank[] { }, new[] { Rank.Ten, Rank.Ten, Rank.Ten }, 30)]
        [InlineData(new[] { Rank.Ace }, new[] { Rank.Ace }, 12)]
        [InlineData(new[] { Rank.Ace }, new[] { Rank.Ten }, 21)]
        [InlineData(new[] { Rank.Ace }, new[] { Rank.Ace, Rank.Ace }, 13)]
        [InlineData(new[] { Rank.Ace }, new[] { Rank.Ten, Rank.Ace }, 12)]
        [InlineData(new[] { Rank.Ace }, new[] { Rank.Ten, Rank.Ten }, 21)]
        [InlineData(new[] { Rank.Ace }, new[] { Rank.Ten, Rank.Ten, Rank.Ace }, 22)]
        [InlineData(new[] { Rank.Ten }, new[] { Rank.Ace }, 21)]
        [InlineData(new[] { Rank.Ten }, new[] { Rank.Ten }, 20)]
        [InlineData(new[] { Rank.Ten }, new[] { Rank.Ace, Rank.Ace }, 12)]
        [InlineData(new[] { Rank.Ten }, new[] { Rank.Ten, Rank.Ace }, 21)]
        [InlineData(new[] { Rank.Ten }, new[] { Rank.Ten, Rank.Ten }, 30)]
        [InlineData(new[] { Rank.Ten }, new[] { Rank.Ten, Rank.Ten, Rank.Ace }, 31)]
        [InlineData(new[] { Rank.Ten, Rank.Ace }, new[] { Rank.Ace }, 12)]
        [InlineData(new[] { Rank.Ten, Rank.Ace }, new[] { Rank.Ten }, 21)]
        [InlineData(new[] { Rank.Ten, Rank.Ace }, new[] { Rank.Ace, Rank.Ace }, 13)]
        [InlineData(new[] { Rank.Ten, Rank.Ace }, new[] { Rank.Ten, Rank.Ace }, 22)]
        [InlineData(new[] { Rank.Ten, Rank.Ace }, new[] { Rank.Ten, Rank.Ten }, 31)]
        [InlineData(new[] { Rank.Ten, Rank.Ace }, new[] { Rank.Ten, Rank.Ten, Rank.Ace }, 32)]
        [InlineData(new[] { Rank.Ten, Rank.Ten }, new[] { Rank.Ace }, 21)]
        [InlineData(new[] { Rank.Ten, Rank.Ten }, new[] { Rank.Ten }, 30)]
        [InlineData(new[] { Rank.Ten, Rank.Ten }, new[] { Rank.Ace, Rank.Ace }, 22)]
        [InlineData(new[] { Rank.Ten, Rank.Ten }, new[] { Rank.Ten, Rank.Ace }, 31)]
        [InlineData(new[] { Rank.Ten, Rank.Ten }, new[] { Rank.Ten, Rank.Ten }, 40)]
        [InlineData(new[] { Rank.Ten, Rank.Ten }, new[] { Rank.Ten, Rank.Ten, Rank.Ace }, 41)]
        public void Hand_DataDrivenTests(Rank[] initialRanks, Rank[] newRanks, int expectedValue)
        {
            var cards = initialRanks.Select(rank => new Card(Suit.Hearts, rank)).ToList();
            var hand = new Hand(cards);

            foreach (var newRank in newRanks)
            {
                hand.AddCard(new Card(Suit.Clubs, newRank));
            }
            var handValue = hand.HandValue;

            Assert.Equal(expectedValue, handValue);
        }
    }
}