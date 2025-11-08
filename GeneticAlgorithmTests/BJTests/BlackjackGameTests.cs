using System;
using System.Reflection;
using Demo.Demos.BJ;
using GeneticAlgorithm.GeneralGenetic;
using Xunit.Sdk;

namespace GeneticAlgorithmTests.BJTests
{
    public class BlackjackGameTests
    {
        private static readonly StrategyTable ConservativeStrategy = BuildConservativeStrategyTable();

        [Fact]
        public async Task RunSingleGame_NoExceptionsThrown()
        {
            var shoe = new Shoe(DeckFactory.CreateShuffledDecks(6, RandomProvider.Shared).Cards, redCardPosition: 100);
            var game = new BlackjackGameAuto(shoe, new StrategyTable(ConservativeStrategy));
            await game.PlayGameAsync();
        }

        [Fact]
        public async Task PlayGameAsync_LongRun_CompletesWithoutErrors()
        {
            const int gamesToPlay = 10_000;
            var random = new Random(2024);

            for (var i = 0; i < gamesToPlay; i++)
            {
                var shoe = CreateRandomShoe(random);
                var game = new BlackjackGameAuto(shoe, new StrategyTable(ConservativeStrategy));

                try
                {
                    await game.PlayGameAsync();
                }
                catch (Exception ex)
                {
                    var drawCountField = typeof(Shoe).GetField("_currentPosition", BindingFlags.NonPublic | BindingFlags.Instance);
                    var drawCount = drawCountField is null ? -1 : (int)drawCountField.GetValue(shoe)!;
                    throw new XunitException(
                        $"Game failed at iteration {i}. Drawn cards: {drawCount} of {shoe.Cards.Count}. Red card at {shoe.RedCardPosition}. {ex.Message}");
                }

                Assert.True(game.GameFinished);
                Assert.True(shoe.IsShoeEmpty || shoe.IsRedCardRaised);
                Assert.All(game.PlayerHands, hand => Assert.True(hand.IsCompleted));
            }
        }

        private static StrategyTable BuildConservativeStrategyTable()
        {
            var actions = new BlackjackAction[StrategyTable.StrategySize];

            for (var dealer = 0; dealer < StrategyTable.DealerColumnCount; dealer++)
            {
                for (var total = StrategyTable.MinPlayerTotal; total <= StrategyTable.MaxPlayerTotal; total++)
                {
                    var offset = (total - StrategyTable.MinPlayerTotal) * StrategyTable.DealerColumnCount + dealer;
                    actions[offset] = total < 17 ? BlackjackAction.Hit : BlackjackAction.Stand;

                    var softOffset = StrategyTable.HardSectionSize + offset;
                    actions[softOffset] = total < 18 ? BlackjackAction.Hit : BlackjackAction.Stand;
                }
            }

            var pairOffset = StrategyTable.HardSectionSize + StrategyTable.SoftSectionSize;
            for (var i = 0; i < StrategyTable.PairSectionSize; i++)
            {
                actions[pairOffset + i] = BlackjackAction.Stand;
            }

            return new StrategyTable(actions);
        }

        private static Shoe CreateRandomShoe(Random random)
        {
            var deckRandom = new Random(random.Next());
            var deck = DeckFactory.CreateShuffledDecks(6, new RandomProvider(deckRandom));
            const int safetyMargin = 80;
            var redCardPosition = Math.Max(deck.Cards.Count - safetyMargin, safetyMargin);
            return new Shoe(deck.Cards, redCardPosition);
        }
    }
}
