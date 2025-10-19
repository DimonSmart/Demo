namespace Demo.Demos.BJ
{
    public class Shoe : Deck
    {
        private readonly int _redCardPosition;
        private int _currentPosition;

        public Shoe(IReadOnlyList<Card> cards, int redCardPosition)
        {
            Cards = cards;
            _redCardPosition = redCardPosition;
            _currentPosition = 0;
        }

        public int RedCardPosition => _redCardPosition;

        public Card TakeNextCard()
        {
            if (_currentPosition >= Cards.Count)
                throw new InvalidOperationException("No more cards can be drawn from this shoe.");

            return Cards[_currentPosition++];
        }

        public bool IsRedCardRaised => _currentPosition >= _redCardPosition;

        public bool IsShoeEmpty => _currentPosition >= Cards.Count;
    }
}
