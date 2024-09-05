using Demo.Demos.TSM.GeneralGenetic;

namespace Demo.Demos.BJ
{
    public static class DeckFactory
    {
        private static IList<Card> CreateOrderedDecks(int numberOfDecks)
        {
            var cards = new List<Card>(numberOfDecks * 4 * 13);
            for (var i = 0; i < numberOfDecks; i++)
            {
                foreach (Suit suit in Enum.GetValues(typeof(Suit)))
                {
                    foreach (Rank rank in Enum.GetValues(typeof(Rank)))
                    {
                        cards.Add(new Card(suit, rank));
                    }
                }
            }
            return cards;
        }

        public static Deck CreateShuffledDecks(int numberOfDecks, IRandomProvider randomProvider)
        {
            var cards = CreateOrderedDecks(numberOfDecks);
            Shuffle(cards, randomProvider);
            return new Deck(cards);
        }

        // Shuffle the deck using Fisher-Yates shuffle algorithm
        private static void Shuffle(IList<Card> cards, IRandomProvider randomProvider)
        {
            var n = cards.Count;
            while (n > 1)
            {
                n--;
                var k = randomProvider.Next(n + 1);
                var value = cards[k];
                cards[k] = cards[n];
                cards[n] = value;
            }
        }
    }
}
