using Demo.Demos.BJ;
using Demo.Demos.TSM.GeneralGenetic;

namespace GeneticAlgorithmTests.BJTests
{
    public class BlackjackGameTests
    {
        [Fact]
        public void RunSingleGame_NoExceptionsThrown()
        {
            var shoe = new Shoe(DeckFactory.CreateShuffledDecks(6, RandomProvider.Shared).Cards, redCardPosition: 100);
            var game = new BlackjackGameAuto(shoe, GetStrategyTable(), logger: ConsoleLogger.Shared);
         //   var game = new BlackjackGameOLD(shoe, GetStrategyTable(), logger: null);
            game.PlayGame(); // .Play();
        }

        private static StrategyTable GetStrategyTable() =>
            new(Enumerable.Range(0, 22 * 11 + 22 * 11 + 14 * 11)
                .Select(_ => (BlackjackAction)RandomProvider.Shared.Next(5))
                .ToArray());
    }
}