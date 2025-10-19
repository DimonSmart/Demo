using GeneticAlgorithm.GeneralGenetic;

namespace TSMDemo.Demos.TSM;

public class TsmChromosomeCrossoverer : IChromosomeCrossoverer<TsmChromosome>
{
    private readonly IRandomProvider _randomProvider;

    public TsmChromosomeCrossoverer(IRandomProvider? randomProvider = null)
    {
        _randomProvider = randomProvider ?? RandomProvider.Shared;
    }

    public void ApplyCrossover(TsmChromosome recipient, TsmChromosome donor)
    {
        var length = recipient.Cities.Length;
        var crossingPosition = _randomProvider.Next(length - 1) + 1;

        var recipientLeft = recipient.Cities.AsSpan(0, crossingPosition);
        var donorRight = donor.Cities.AsSpan(crossingPosition);

        var recipientLeftSet = new HashSet<int>(recipientLeft.ToArray());

        var targetRightPart = new List<int>();

        foreach (var city in donorRight)
        {
            if (!recipientLeftSet.Contains(city))
                targetRightPart.Add(city);
        }

        var combinedElements = new HashSet<int>(recipientLeftSet);
        combinedElements.UnionWith(targetRightPart);

        var allCitiesSet = new HashSet<int>(recipient.Cities);
        allCitiesSet.ExceptWith(combinedElements);

        targetRightPart.AddRange(allCitiesSet);

        var recipientRight = recipient.Cities.AsSpan(crossingPosition);
        targetRightPart.CopyTo(recipientRight);
    }
}
