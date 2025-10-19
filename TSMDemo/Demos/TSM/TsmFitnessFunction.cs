using GeneticAlgorithm.GeneralGenetic;

namespace TSMDemo.Demos.TSM;

public class TsmFitnessFunction : IFitnessFunction<TsmChromosome>
{
    private readonly TsmProblemData _tsmProblemData;

    public TsmFitnessFunction(TsmProblemData tsmProblemData)
    {
        _tsmProblemData = tsmProblemData;
    }

    public int CalculateFitness(TsmChromosome chromosome)
    {
        var length = 0.0;
        for (var i = 0; i < _tsmProblemData.Cities.Length - 1; i++)
        {
            double a = _tsmProblemData.Cities[chromosome.Cities[i + 1]].X - _tsmProblemData.Cities[chromosome.Cities[i]].X;
            double b = _tsmProblemData.Cities[chromosome.Cities[i + 1]].Y - _tsmProblemData.Cities[chromosome.Cities[i]].Y;
            length += Math.Sqrt(a * a + b * b);
        }

        return (int)length;
    }
}
