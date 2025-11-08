using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Demo.Demos.BJ;
using GeneticAlgorithm.GeneralGenetic;
using Xunit.Sdk;

namespace GeneticAlgorithmTests.BJTests
{
    public class BlackjackGameTests
    {
        [Fact]
        public async Task RunSingleGame_NoExceptionsThrown()
        {
            var shoe = new Shoe(DeckFactory.CreateShuffledDecks(6, RandomProvider.Shared).Cards, redCardPosition: 100);
            var game = new BlackjackGameAuto(shoe, new StrategyTable(BuildConservativeStrategyTable()));
            await game.PlayGameAsync();
        }

        [Fact]
        public async Task PlayGameAsync_LongRun_CompletesWithoutErrors()
        {
            const int simulationCount = 16;
            const int gameTimeoutSeconds = 5;
            var random = new Random(2024);
            var randomLock = new object();

            Random CreateScopedRandom()
            {
                lock (randomLock)
                {
                    return new Random(random.Next());
                }
            }

            const int stressTestDeckCount = 6;

            var concurrencyLimit = Environment.ProcessorCount;
            using var semaphore = new SemaphoreSlim(concurrencyLimit, concurrencyLimit);
            var simulations = Enumerable.Range(0, simulationCount).Select(async simulationIndex =>
            {
                await semaphore.WaitAsync();

                try
                {
                    var shoe = CreateRandomShoe(CreateScopedRandom(), stressTestDeckCount);
                    var game = new BlackjackGameAuto(shoe, new StrategyTable(CreateRandomStrategy(CreateScopedRandom())));

                    var playTask = game.PlayGameAsync();
                    var completedTask = await Task.WhenAny(playTask, Task.Delay(TimeSpan.FromSeconds(gameTimeoutSeconds)));

                    try
                    {
                        if (completedTask != playTask)
                        {
                            throw new TimeoutException($"The game simulation exceeded {gameTimeoutSeconds} seconds.");
                        }

                        await playTask;
                    }
                    catch (Exception ex)
                    {
                        var drawCountField = typeof(Shoe).GetField("_currentPosition", BindingFlags.NonPublic | BindingFlags.Instance);
                        var drawCount = drawCountField is null ? -1 : (int)drawCountField.GetValue(shoe)!;
                        throw new XunitException(
                            $"Game failed for simulation {simulationIndex}. Drawn cards: {drawCount} of {shoe.Cards.Count}. Red card at {shoe.RedCardPosition}. {ex.Message}");
                    }

                    Assert.True(game.GameFinished);
                    Assert.True(shoe.IsShoeEmpty || shoe.IsRedCardRaised);
                    Assert.All(game.PlayerHands,
                        hand => Assert.True(hand.IsCompleted || shoe.IsShoeEmpty || shoe.IsRedCardRaised));
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(simulations);
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

        private static BlackjackAction[] CreateRandomStrategy(Random random)
        {
            var actions = new BlackjackAction[StrategyTable.StrategySize];
            var possibleActions = Enum.GetValues<BlackjackAction>();

            for (var i = 0; i < actions.Length; i++)
            {
                actions[i] = possibleActions[random.Next(possibleActions.Length)];
            }

            return actions;
        }

        private static Shoe CreateRandomShoe(Random random, int deckCount = 6)
        {
            var deckRandom = new Random(random.Next());
            var deck = DeckFactory.CreateShuffledDecks(deckCount, new RandomProvider(deckRandom));
            const int safetyMargin = 80;
            var redCardPosition = Math.Max(deck.Cards.Count - safetyMargin, safetyMargin);
            return new Shoe(deck.Cards, redCardPosition);
        }
    }
}
