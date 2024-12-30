using GeneticAlgorithm.GeneralGenetic;

namespace BJTableCalculator
{
    public class BlackjackChromosomeMutator : IChromosomeMutator<BlackjackChromosome>
    {
        private readonly IRandomProvider _randomProvider;

        public BlackjackChromosomeMutator(IRandomProvider? randomProvider = null)
        {
            _randomProvider = randomProvider ?? RandomProvider.Shared;
        }

        public void Mutate(BlackjackChromosome chromosome)
        {

        }
    }
}