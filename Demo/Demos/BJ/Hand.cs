namespace Demo.Demos.BJ
{
    public class Hand : ICardHolder, IHandValues
    {
        protected internal List<Card> _cards;
        public IReadOnlyList<Card> Cards => _cards.AsReadOnly();
        public int HandValue { get; private set; }
        private int _acesCount;
      

        public Hand(IEnumerable<Card> initialCards)
        {
            _cards = initialCards.ToList();
            CalculateHandValue();
        }

        public void AddCard(Card card)
        {
            _cards.Add(card);
            if (card.Rank == Rank.Ace)
            {
                _acesCount++;
            }
            UpdateHandValue(card);
            AdjustForAces();
        }

        protected void CalculateHandValue()
        {
            HandValue = 0;
            _acesCount = 0;

            foreach (var card in _cards)
            {
                HandValue += card.Value;
                if (card.Rank == Rank.Ace)
                {
                    _acesCount++;
                }
            }
        }

        private void UpdateHandValue(Card card)
        {
            HandValue += card.Value;
            AdjustForAces();
        }

        private void AdjustForAces()
        {
            while (HandValue > 21 && _acesCount > 0)
            {
                HandValue -= 10;
                _acesCount--;
            }
        }

        public bool IsBusted => HandValue > 21;

        public bool IsSoft => _acesCount > 0 && HandValue <= 21;

        public bool IsPair => _cards.Count == 2 && _cards[0].Rank == _cards[1].Rank;
    }

    public class PlayerHand(IEnumerable<Card> initialCards, decimal initialBet) : Hand(initialCards)
    {
        public decimal Bet { get; set; } = initialBet;
        public PlayerHand Split()
        {
            if (!IsPair) throw new InvalidOperationException("Cannot split a hand that is not a pair.");

            var cardToRemove = _cards[1];
            _cards.RemoveAt(1);
            CalculateHandValue();

            return new PlayerHand(new List<Card> { cardToRemove }, Bet);
        }
    }
}
