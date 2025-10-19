using GeneticAlgorithm.GeneralGenetic;
using TSMDemo.Demos.TSM;

namespace BJTableCalculator
{
    public class BlackjackFitnessFunction : IFitnessFunction<BlackjackChromosome>
    {
        private readonly TsmProblemData _tsmProblemData;

        public BlackjackFitnessFunction(TsmProblemData tsmProblemData)
        {
            _tsmProblemData = tsmProblemData;
        }

        public int CalculateFitness(BlackjackChromosome chromosome)
        {
            throw new NotImplementedException();
        }
    }
}
