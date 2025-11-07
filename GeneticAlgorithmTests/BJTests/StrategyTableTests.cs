using Demo.Demos.BJ;
using System.Linq;

namespace GeneticAlgorithmTests.BJTests
{
    public class StrategyTableTests
    {
        [Fact]
        public void GetAction_PairOfFaceCards_UsesPairSectionWithoutErrors()
        {
            var strategy = Enumerable.Repeat(BlackjackAction.Hit, StrategyTable.StrategySize).ToArray();
            var table = new StrategyTable(strategy);
            var hand = new PlayerHand(new[]
            {
                new Card(Suit.Spades, Rank.King),
                new Card(Suit.Hearts, Rank.King)
            }, 10m);

            var action = table.GetAction(hand, dealerCardValue: 10);

            Assert.Equal(BlackjackAction.Hit, action);
        }
    }
}
