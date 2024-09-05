using Demo.Demos.BJ;

namespace GeneticAlgorithmTests.BJTests
{
    public class HandTests
    {
        [Fact]
        public void Hand_CalculateInitialValue_CorrectlyCalculatesValue()
        {
            // Arrange
            var cards = new List<Card>
        {
            new (Suit.Hearts, Rank.Ten),
            new (Suit.Spades, Rank.Seven)
        };
            var hand = new Hand(cards);

            // Act
            var handValue = hand.HandValue;

            // Assert
            Assert.Equal(17, handValue);
        }

        [Fact]
        public void Hand_AddCard_UpdatesHandValueCorrectly()
        {
            // Arrange
            var cards = new List<Card>
        {
            new (Suit.Hearts, Rank.Ten),
            new (Suit.Spades, Rank.Six)
        };
            var hand = new Hand(cards);

            // Act
            hand.AddCard(new Card(Suit.Clubs, Rank.Two));
            var handValue = hand.HandValue;

            // Assert
            Assert.Equal(18, handValue);
        }

        [Fact]
        public void Hand_AddAce_CorrectlyAdjustsValueForAce()
        {
            // Arrange
            var cards = new List<Card>
        {
            new(Suit.Hearts, Rank.Ten),
            new(Suit.Spades, Rank.Seven)
        };
            var hand = new Hand(cards);

            // Act
            hand.AddCard(new Card(Suit.Clubs, Rank.Ace));
            var handValue = hand.HandValue;

            // Assert
            Assert.Equal(18, handValue); // Ace should count as 1 to prevent bust
        }

        [Fact]
        public void Hand_AddTwoAces_CorrectlyAdjustsValueForAce()
        {
            // Arrange
            var cards = new List<Card>
        {
            new(Suit.Hearts, Rank.Ten),
            new(Suit.Spades, Rank.Seven)
        };
            var hand = new Hand(cards);

            // Act
            hand.AddCard(new Card(Suit.Clubs, Rank.Ace));
            hand.AddCard(new Card(Suit.Hearts, Rank.Ace));
            var handValue = hand.HandValue;

            // Assert
            Assert.Equal(19, handValue); // Ace should count as 1 to prevent bust
        }

        [Fact]
        public void Hand_BustCheck_ReturnsTrueWhenBusted()
        {
            // Arrange
            var cards = new List<Card>
        {
            new (Suit.Hearts, Rank.King),
            new (Suit.Spades, Rank.Queen)
        };
            var hand = new Hand(cards);

            // Act
            hand.AddCard(new Card(Suit.Clubs, Rank.Five));
            var isBusted = hand.IsBusted;

            // Assert
            Assert.True(isBusted);
        }
    }
}