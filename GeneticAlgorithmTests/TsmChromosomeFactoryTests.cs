using Demo.Demos.TSM;

namespace GeneticAlgorithmTests
{
    public class TsmChromosomeFactoryTests
    {
        [Fact]
        public void Create_ShouldReturnChromosomeWithCorrectNumberOfCities()
        {
            // Arrange
            var citiesCount = 5;
            var factory = new TsmChromosomeFactory(citiesCount);

            // Act
            var chromosome = factory.Create();

            // Assert
            Assert.NotNull(chromosome);
            Assert.Equal(citiesCount, chromosome.Cities.Length);
        }

        [Fact]
        public void Create_ShouldReturnDifferentChromosomesWhenCalledMultipleTimes()
        {
            // Arrange
            int citiesCount = 5;
            var factory = new TsmChromosomeFactory(citiesCount);

            // Act
            var chromosome1 = factory.Create();
            var chromosome2 = factory.Create();

            // Assert
            Assert.NotEqual(chromosome1.Cities, chromosome2.Cities);
        }
    }
}
