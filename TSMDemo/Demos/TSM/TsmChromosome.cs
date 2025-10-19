using GeneticAlgorithm.GeneralGenetic;

namespace TSMDemo.Demos.TSM;

public sealed class TsmChromosome : IChromosome<TsmChromosome>
{
    public int[] Cities { get; private set; }

    private TsmChromosome(IEnumerable<int> cities) => Cities = cities.ToArray();

    public static TsmChromosome CreateTsmChromosome(IEnumerable<int> cities) =>
        new(cities);

    public int CompareTo(TsmChromosome? other) =>
        other == null ? 1 : Cities.SequenceEqual(other.Cities) ? 0 : 1;

    public override string ToString() => string.Join(",", Cities);
}
