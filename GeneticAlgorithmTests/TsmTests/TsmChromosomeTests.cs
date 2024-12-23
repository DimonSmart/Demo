using static Demo.Demos.TSM.TsmChromosome;

namespace GeneticAlgorithmTests.TsmTests
{
    public class TsmChromosomeTests
    {
        [Fact]
        public void TsmChromosome_CompareTo_ShouldReturnZeroForEqualChromosomes()
        {
            // Arrange
            var chromosome1 = CreateTsmChromosome([0, 1, 2, 3, 4, 5]);
            var chromosome2 = CreateTsmChromosome([0, 1, 2, 3, 4, 5]);

            // Act
            var comparison = chromosome1.CompareTo(chromosome2);

            // Assert
            Assert.Equal(0, comparison);
        }
    }
}
