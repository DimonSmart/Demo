namespace GeneticAlgorithm.GeneralGenetic
{
    public interface IChromosomeFactory<T> where T : class, IChromosome<T>
    {
        T Create();
        T CreateCopy(T chromosomeWithScore);
    }
}