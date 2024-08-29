namespace Demo.Demos.TSM.GeneralGenetic
{
    public interface IChromosomeCrossoverer<T> where T : class, IChromosome<T>
    {
        void ApplyCrossover(T recipient, T donor);
    }
}