namespace Demo.Demos.TSM.GeneralGenetic
{
    public class GeneticAlgorithmSettings
    {
        public int BestChromosomes { get; set; } = 50;

        public int CrossoverChromosomes { get; set; } = 30;

        public int MutationChromosomes { get; set; } = 20;

        public int Population { get; set; } = 100;

        /// <summary>
        /// Validates that the sum of BestChromosomes, CrossoverChromosomes, and MutationChromosomes
        /// does not exceed the total Population.
        /// </summary>
        public void Validate()
        {
            int totalChromosomes = BestChromosomes + CrossoverChromosomes + MutationChromosomes;
            if (totalChromosomes > Population)
            {
                throw new InvalidOperationException("Total chromosome count exceeds the population size.");
            }
        }
    }
}
