using GeneticAlgorithm.GeneralGenetic;

namespace Demo.Demos.TSM
{
    public class TsmChromosomeMutator : IChromosomeMutator<TsmChromosome>
    {
        private readonly IRandomProvider _randomProvider;

        public TsmChromosomeMutator(IRandomProvider? randomProvider = null)
        {
            _randomProvider = randomProvider ?? RandomProvider.Shared;
        }

        public void Mutate(TsmChromosome chromosome)
        {
            var index1 = _randomProvider.Next(chromosome.Cities.Length);
            var index2 = _randomProvider.Next(chromosome.Cities.Length);

            var temp = chromosome.Cities[index1];
            chromosome.Cities[index1] = chromosome.Cities[index2];
            chromosome.Cities[index2] = temp;
        }
    }
}