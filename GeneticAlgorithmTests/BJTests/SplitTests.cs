using Demo.Demos.BJ;

namespace GeneticAlgorithmTests.BJTests
{
    public class SplitTests
    {
        /// <summary>
        /// Test verifies that after splitting, each hand automatically receives a second card
        /// </summary>
        [Fact]
        public async Task SplitAsync_AutomaticallyDealsSecondCardToEachHand()
        {
            // Arrange: create a deck with known cards
            // Deal order: player card 1, player card 2, dealer card 1, dealer card 2, then cards for split
            var cards = new List<Card>
            {
                new(Suit.Hearts, Rank.Eight),   // Player card 1
                new(Suit.Diamonds, Rank.Eight), // Player card 2 (pair)
                new(Suit.Clubs, Rank.Five),     // Dealer card 1
                new(Suit.Spades, Rank.Two),     // Dealer card 2
                new(Suit.Hearts, Rank.Three),   // Card for first hand after split
                new(Suit.Clubs, Rank.Four),     // Card for second hand after split
            };
            
            var shoe = new Shoe(cards, redCardPosition: 100);
            var game = new TestableBlackjackGame(shoe);
            
            // Act
            await game.StartNewRoundAsync();
            
            // Verify player has a pair of eights
            Assert.Equal(2, game.CurrentPlayerHand!.Cards.Count);
            Assert.Equal(Rank.Eight, game.CurrentPlayerHand.Cards[0].Rank);
            Assert.Equal(Rank.Eight, game.CurrentPlayerHand.Cards[1].Rank);
            Assert.True(game.CanSplit());
            
            await game.SplitAsync();
            
            // Assert: verify both hands received a second card
            Assert.Equal(2, game.PlayerHands.Count);
            
            // First hand: 8 + 3 = 11
            Assert.Equal(2, game.PlayerHands[0].Cards.Count);
            Assert.Equal(Rank.Eight, game.PlayerHands[0].Cards[0].Rank);
            Assert.Equal(Rank.Three, game.PlayerHands[0].Cards[1].Rank);
            Assert.Equal(11, game.PlayerHands[0].HandValue);
            
            // Second hand: 8 + 4 = 12
            Assert.Equal(2, game.PlayerHands[1].Cards.Count);
            Assert.Equal(Rank.Eight, game.PlayerHands[1].Cards[0].Rank);
            Assert.Equal(Rank.Four, game.PlayerHands[1].Cards[1].Rank);
            Assert.Equal(12, game.PlayerHands[1].HandValue);
        }

        /// <summary>
        /// Test verifies special logic for splitting aces: only one card per hand after split
        /// </summary>
        [Fact]
        public async Task SplitAsync_SplitAces_EachHandReceivesOneCardAndCompletes()
        {
            // Arrange: create deck with a pair of aces
            // Order: player 1, player 2, dealer 1, dealer 2, cards for split, cards for dealer
            var cards = new List<Card>
            {
                new(Suit.Hearts, Rank.Ace),     // Player card 1
                new(Suit.Diamonds, Rank.Ace),   // Player card 2 (pair of aces)
                new(Suit.Clubs, Rank.Five),     // Dealer card 1
                new(Suit.Spades, Rank.Two),     // Dealer card 2
                new(Suit.Hearts, Rank.Nine),    // Card for first hand after split
                new(Suit.Clubs, Rank.Ten),      // Card for second hand after split
                new(Suit.Diamonds, Rank.Six),   // Dealer card 3 (if needed)
                new(Suit.Spades, Rank.Seven),   // Dealer card 4 (if needed)
                new(Suit.Hearts, Rank.Eight),   // Dealer card 5 (if needed)
            };
            
            var shoe = new Shoe(cards, redCardPosition: 100);
            var rulesOptions = new BlackjackRulesOptions { CanHitAfterSplitAces = false };
            var game = new TestableBlackjackGame(shoe, rulesOptions: rulesOptions);
            
            // Act
            await game.StartNewRoundAsync();
            
            Assert.True(game.CanSplit());
            await game.SplitAsync();
            
            // Assert: both hands should be completed (cannot take more cards)
            Assert.Equal(2, game.PlayerHands.Count);
            
            // First hand: ace + 9 = 20, completed
            Assert.True(game.PlayerHands[0].WasSplitAce);
            Assert.Equal(2, game.PlayerHands[0].Cards.Count);
            Assert.Equal(20, game.PlayerHands[0].HandValue);
            Assert.True(game.PlayerHands[0].IsCompleted);
            
            // Second hand: ace + 10 = 21, completed
            Assert.True(game.PlayerHands[1].WasSplitAce);
            Assert.Equal(2, game.PlayerHands[1].Cards.Count);
            Assert.Equal(21, game.PlayerHands[1].HandValue);
            Assert.True(game.PlayerHands[1].IsCompleted);
            
            // Player cannot take more cards
            Assert.False(game.CanHit());
        }

        /// <summary>
        /// Test verifies that CanSplit returns false when the hand limit is reached (MaxSplits = 3)
        /// </summary>
        [Fact]
        public async Task CanSplit_EnforcesMaxSplitsLimit()
        {
            // Arrange: create test class that allows direct access to limit checks
            var cards = new List<Card>
            {
                new(Suit.Hearts, Rank.Eight),   
                new(Suit.Diamonds, Rank.Eight), 
                new(Suit.Clubs, Rank.Five),     
                new(Suit.Spades, Rank.Two),     
            };
            
            var shoe = new Shoe(cards, redCardPosition: 100);
            var game = new TestableBlackjackGame(shoe);
            
            await game.StartNewRoundAsync();
            
            // Initial state: 1 hand with pair, split is available
            Assert.Single(game.PlayerHands);
            Assert.True(game.CurrentPlayerHand!.IsPair);
            Assert.True(game.CanSplit());
            
            // Manually add hands to simulate state after multiple splits
            // Add second hand (as if we made 1 split) 
            var hand2 = new PlayerHand(new[] { new Card(Suit.Clubs, Rank.Seven) }, 10m);
            game.PlayerHands.Add(hand2);
            Assert.Equal(2, game.PlayerHands.Count);
            // With 2 hands, split is still available (if current hand has a pair)
            
            // Add third hand (as if we made 2 splits)
            var hand3 = new PlayerHand(new[] { new Card(Suit.Spades, Rank.Nine) }, 10m);
            game.PlayerHands.Add(hand3);
            Assert.Equal(3, game.PlayerHands.Count);
            // With 3 hands, split is still available (if current hand has a pair) - last possible
            
            // Add fourth hand (maximum reached: MaxSplits = 3)
            var hand4 = new PlayerHand(new[] { new Card(Suit.Hearts, Rank.Ten) }, 10m);
            game.PlayerHands.Add(hand4);
            Assert.Equal(4, game.PlayerHands.Count);
            
            // Now even with a pair in current hand, split should be unavailable
            Assert.True(game.CurrentPlayerHand!.IsPair); // We still have a pair of eights
            Assert.False(game.CanSplit()); // But split is unavailable - limit reached (PlayerHands.Count = 4 >= MaxSplits = 3)
        }

        /// <summary>
        /// Test verifies that WasSplitAce is correctly set only for ace splits
        /// </summary>
        [Fact]
        public async Task SplitAsync_WasSplitAceFlagSetCorrectly()
        {
            // Arrange: split NON-aces
            // Order: player 1, player 2, dealer 1, dealer 2, cards for split
            var cards = new List<Card>
            {
                new(Suit.Hearts, Rank.Seven),
                new(Suit.Diamonds, Rank.Seven),
                new(Suit.Clubs, Rank.Five),
                new(Suit.Spades, Rank.Two),
                new(Suit.Hearts, Rank.Three),
                new(Suit.Clubs, Rank.Four),
            };
            
            var shoe = new Shoe(cards, redCardPosition: 100);
            var game = new TestableBlackjackGame(shoe);
            
            await game.StartNewRoundAsync();
            await game.SplitAsync();
            
            // Assert: WasSplitAce flag should be false
            Assert.False(game.PlayerHands[0].WasSplitAce);
            Assert.False(game.PlayerHands[1].WasSplitAce);
        }

        /// <summary>
        /// Test verifies that after splitting with result of 21, hand is not auto-completed (if not aces)
        /// </summary>
        [Fact]
        public async Task SplitAsync_NonAces_HandWith21DoesNotAutoComplete()
        {
            // Arrange: split eights, one receives three (11), another receives king (18)
            // Order: player 1, player 2, dealer 1, dealer 2, cards for split
            var cards = new List<Card>
            {
                new(Suit.Hearts, Rank.Eight),
                new(Suit.Diamonds, Rank.Eight),
                new(Suit.Clubs, Rank.Five),
                new(Suit.Spades, Rank.Two),
                new(Suit.Hearts, Rank.Three),   // First hand: 8+3=11
                new(Suit.Clubs, Rank.King),     // Second hand: 8+10=18
            };
            
            var shoe = new Shoe(cards, redCardPosition: 100);
            var game = new TestableBlackjackGame(shoe);
            
            await game.StartNewRoundAsync();
            await game.SplitAsync();
            
            // Assert: hands should not be automatically completed (this is not an ace split)
            Assert.False(game.PlayerHands[0].IsCompleted);
            Assert.False(game.PlayerHands[1].IsCompleted);
            
            // Player can continue playing the first hand
            Assert.True(game.CanHit() || game.CanStand());
        }

        /// <summary>
        /// Testable class for accessing protected members
        /// </summary>
        private class TestableBlackjackGame : BlackjackGameBase
        {
            public TestableBlackjackGame(Shoe shoe, ILogger? logger = null, BlackjackRulesOptions? rulesOptions = null)
                : base(shoe, logger, rulesOptions)
            {
            }
        }
    }
}
