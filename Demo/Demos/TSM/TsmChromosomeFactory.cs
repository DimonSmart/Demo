using Demo.Demos.TSM.GeneralGenetic;

namespace Demo.Demos.TSM
{
    public class TsmChromosomeFactory : IChromosomeFactory<TsmChromosome>
    {
        private readonly int _citiesCount;
        private readonly IRandomProvider _randomProvider;

        public TsmChromosomeFactory(int citiesCount, IRandomProvider? randomProvider = null)
        {
            _citiesCount = citiesCount;
            _randomProvider = randomProvider ?? new RandomProvider();
        }

        public TsmChromosome Create()
        {
            var cities = Enumerable.Range(0, _citiesCount)
                                   .OrderBy(_ => _randomProvider.Next())
                                   .ToArray();

            return TsmChromosome.CreateTsmChromosome(cities);
        }

        public TsmChromosome CreateCopy(TsmChromosome tsmChromosome)
        {
            return TsmChromosome.CreateTsmChromosome(tsmChromosome.Cities);
        }
    }
}