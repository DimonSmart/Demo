namespace Demo.Demos.TSM.GeneralGenetic
{
    public interface IChromosomeMutator<T> where T : class, IChromosome<T>
    {
        void Mutate(T chromosome);
    }
}