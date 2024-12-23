namespace GeneticAlgorithm.GeneralGenetic
{
    public interface IChromosomeMutator<T> where T : class, IChromosome<T>
    {
        void Mutate(T chromosome);
    }
}