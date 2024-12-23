using System.Text;

namespace GeneticAlgorithm.GeneralGenetic
{
    public class ChromosomeRanges
    {
        private readonly GeneticAlgorithmSettings _settings;

        public ChromosomeRanges(GeneticAlgorithmSettings settings)
        {
            _settings = settings;
            _settings.Validate();

            BestChromosomesStartIndex = 0;
            BestChromosomesCount = _settings.BestChromosomes;
            BestChromosomesEndIndex = BestChromosomesStartIndex + BestChromosomesCount;

            CrossoverChromosomesStartIndex = BestChromosomesStartIndex + BestChromosomesCount;
            CrossoverChromosomesCount = _settings.CrossoverChromosomes;

            MutationChromosomesStartIndex = CrossoverChromosomesStartIndex + CrossoverChromosomesCount;
            MutationChromosomesCount = _settings.MutationChromosomes;

            NewChromosomesStartIndex = MutationChromosomesStartIndex + MutationChromosomesCount;
            NewChromosomesCount = _settings.Population - BestChromosomesCount - CrossoverChromosomesCount - MutationChromosomesCount;
        }

        public int BestChromosomesStartIndex { get; private set; }
        public int BestChromosomesCount { get; private set; }
        public int BestChromosomesEndIndex { get; private set; }

        public int CrossoverChromosomesStartIndex { get; private set; }
        public int CrossoverChromosomesCount { get; private set; }

        public int MutationChromosomesStartIndex { get; private set; }
        public int MutationChromosomesCount { get; private set; }

        public int NewChromosomesStartIndex { get; private set; }
        public int NewChromosomesCount { get; private set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"BestChromosomes [{BestChromosomesStartIndex}, {BestChromosomesStartIndex + BestChromosomesCount - 1}]");
            sb.AppendLine($"CrossoverChromosomes [{CrossoverChromosomesStartIndex}, {CrossoverChromosomesStartIndex + CrossoverChromosomesCount - 1}]");
            sb.AppendLine($"MutationChromosomes [{MutationChromosomesStartIndex}, {MutationChromosomesStartIndex + MutationChromosomesCount - 1}]");
            sb.AppendLine($"NewChromosomes [{NewChromosomesStartIndex}, {NewChromosomesStartIndex + NewChromosomesCount - 1}]");
            return sb.ToString();
        }
    }
}
