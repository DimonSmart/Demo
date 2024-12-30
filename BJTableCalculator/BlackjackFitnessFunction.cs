using Demo.Demos.TSM;
using GeneticAlgorithm.GeneralGenetic;

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