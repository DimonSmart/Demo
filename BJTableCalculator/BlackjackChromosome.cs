using Demo.Demos.BJ;
using GeneticAlgorithm.GeneralGenetic;

namespace BJTableCalculator
{
    public sealed class BlackjackChromosome(StrategyTable strategyTable) : IChromosome<BlackjackChromosome>
    {
        public StrategyTable _strategyTable = new StrategyTable(strategyTable);

        public static BlackjackChromosome CreateBlackJackChromosome(StrategyTable strategyTable) =>
            new(strategyTable);
    }
}