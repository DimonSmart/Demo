namespace GeneticAlgorithm.GeneralGenetic
{
    public class ChromosomeWithScore<T> where T : class, IChromosome<T>
    {
        public required T Chromosome;
        public required int Score;

        public override string ToString()
        {
            return $"Score: {Score}, Chromosome: {Chromosome}";
        }
    }
}