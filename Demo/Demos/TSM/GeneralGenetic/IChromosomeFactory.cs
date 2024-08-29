namespace Demo.Demos.TSM.GeneralGenetic
{
    public interface IChromosomeFactory<T> where T : class, IChromosome<T>
    {
        T Create();
    }
}