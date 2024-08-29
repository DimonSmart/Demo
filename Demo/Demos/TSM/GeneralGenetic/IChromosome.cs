namespace Demo.Demos.TSM.GeneralGenetic
{
    public interface IChromosome<T> where T : class, IChromosome<T>
    {
    }
}