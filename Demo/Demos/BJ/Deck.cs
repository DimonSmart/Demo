namespace Demo.Demos.BJ
{
    public class Deck
    {
        public IReadOnlyList<Card> Cards { get; protected set; }

        public Deck()
        {
            Cards = [];
        }

        public Deck(IEnumerable<Card> cards)
        {
            Cards = cards.ToList();
        }
    }
}
