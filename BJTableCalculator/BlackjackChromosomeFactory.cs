using Demo.Demos.BJ;
using GeneticAlgorithm.GeneralGenetic;

namespace BJTableCalculator
{
    public class BlackjackChromosomeFactory : IChromosomeFactory<BlackjackChromosome>
    {
        private readonly IRandomProvider _randomProvider;

        public BlackjackChromosomeFactory(IRandomProvider? randomProvider = null)
        {
            _randomProvider = randomProvider ?? new RandomProvider();
        }

        public BlackjackChromosome Create()
        {
            var strategyTable = GetStrategyTable();
            return new BlackjackChromosome(strategyTable);
        }
        public BlackjackChromosome CreateCopy(BlackjackChromosome chromosomeWithScore) => throw new NotImplementedException();

        private static StrategyTable GetStrategyTable() =>
            new(Enumerable.Range(0, StrategyTable.StrategySize)
                .Select(_ => (BlackjackAction)RandomProvider.Shared.Next(5))
                .ToArray());
    }
}