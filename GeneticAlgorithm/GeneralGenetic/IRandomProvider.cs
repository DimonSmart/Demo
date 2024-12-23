namespace GeneticAlgorithm.GeneralGenetic
{
    public interface IRandomProvider
    {
        int Next();
        int Next(int length);
    }
}