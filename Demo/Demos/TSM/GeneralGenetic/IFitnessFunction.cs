namespace Demo.Demos.TSM.GeneralGenetic
{
    public interface IFitnessFunction<T> where T : class, IChromosome<T>
    {
        int CalculateFitness(T chromosome);
    }
}
