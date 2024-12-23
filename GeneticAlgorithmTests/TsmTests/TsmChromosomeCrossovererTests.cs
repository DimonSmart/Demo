using Demo.Demos.TSM;
using GeneticAlgorithm.GeneralGenetic;
using Moq;

namespace GeneticAlgorithmTests.TsmTests
{
    public class TsmChromosomeCrossovererTests
    {
        [Fact]
        public void ApplyCrossover_ShouldModifyRecipientChromosome()
        {
            // Arrange
            var recipientCities = new[] { 0, 1, 2, 3, 4, 5 };
            var donorCities = new[] { 5, 4, 3, 2, 1, 0 };
            var recipient = TsmChromosome.CreateTsmChromosome(recipientCities);
            var donor = TsmChromosome.CreateTsmChromosome(donorCities);
            var crossoverer = new TsmChromosomeCrossoverer();

            // Act
            crossoverer.ApplyCrossover(recipient, donor);

            // Assert
            Assert.All(donorCities, city => Assert.Contains(city, recipient.Cities));
        }

        [Fact]
        public void ApplyCrossover_ShouldUseCustomRandomProvider()
        {
            // Arrange
            var mockRandomProvider = new Mock<IRandomProvider>();
            mockRandomProvider.Setup(r => r.Next(It.IsAny<int>())).Returns<int>((max) => 2);
            var recipientCities = new[] { 2, 4, 1, 3, 0, 5 };
            var donorCities = new[] { 5, 4, 3, 2, 1, 0 };
            var recipient = TsmChromosome.CreateTsmChromosome(recipientCities.ToArray());
            var donor = TsmChromosome.CreateTsmChromosome(donorCities.ToArray());
            var crossoverer = new TsmChromosomeCrossoverer(mockRandomProvider.Object);

            // Act
            crossoverer.ApplyCrossover(recipient, donor);

            // Assert
            Assert.NotEqual(recipientCities, recipient.Cities);
            Assert.All(donorCities, city => Assert.Contains(city, recipient.Cities));
        }

        [Fact]
        public void ApplyCrossover_ShouldProduceIdenticalResultsWhenParentsAreIdentical()
        {
            // Arrange
            var cities = new[] { 0, 1, 2, 3, 4, 5 };
            var recipient = TsmChromosome.CreateTsmChromosome(cities.ToArray());
            var donor = TsmChromosome.CreateTsmChromosome(cities.ToArray());
            var crossoverer = new TsmChromosomeCrossoverer();

            // Act
            crossoverer.ApplyCrossover(recipient, donor);

            // Assert
            Assert.Equal(cities, recipient.Cities);
        }
    }
}