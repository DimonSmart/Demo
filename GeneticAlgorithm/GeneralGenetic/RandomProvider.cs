namespace GeneticAlgorithm.GeneralGenetic
{
    public class RandomProvider(Random? random = null) : IRandomProvider
    {
        private readonly Random _random = random ?? Random.Shared;

        public int Next()
        {
            return _random.Next();
        }

        public int Next(int maxValue)
        {
            return _random.Next(maxValue);
        }

        public static readonly RandomProvider Shared = new();
    }
}