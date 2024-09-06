namespace Demo.Demos.BJ
{
    public abstract class BlackjackGameBase
    {
        private const decimal InitialBet = 10m;
        private const decimal SurrenderPenalty = InitialBet / 2;

        protected List<Hand> _playerHands;
        protected Hand _dealerHand;
        protected readonly Shoe _shoe;
        protected readonly ILogger? _logger;

        public int TotalGamesCount { get; protected set; }
        public decimal PlayerMoney { get; protected set; }

        public BlackjackGameBase(Shoe shoe, ILogger? logger = null)
        {
            _shoe = shoe;
            _dealerHand = new Hand(new List<Card>());
            TotalGamesCount = 0;
            _logger = logger;
        }

        public void Play()
        {
            _logger?.Info("Starting the game.");
            while (!_shoe.IsRedCardRaised)
            {
                PlaySingleGame();
                TotalGamesCount++;
                _logger?.Info($"Game {TotalGamesCount} completed. Player's money: {PlayerMoney}");
            }

            _logger?.Info($"Game over! Total games played: {TotalGamesCount}, Player's final money: {PlayerMoney}");
        }

        protected virtual void PlaySingleGame()
        {
            _logger?.Info("Dealing initial cards to player and dealer.");
            _playerHands = new List<Hand> { new Hand(DealInitialCards()) };
            _dealerHand = new Hand(DealInitialCards());

            PlayPlayerHands();
            if (_playerHands.Count > 0 && !_playerHands[0].IsBusted)
            {
                _logger?.Info("Playing dealer's hand.");
                PlayDealerHand();
            }

            EvaluateGameOutcome();
        }

        protected List<Card> DealInitialCards()
        {
            return new List<Card> { _shoe.TakeNextCard(), _shoe.TakeNextCard() };
        }

        protected abstract void PlayPlayerHands();

        protected void PlayDealerHand()
        {
            _logger?.Info("Dealer plays according to the rules.");

            // Dealer must hit until their hand value is 17 or higher.
            while (_dealerHand.HandValue < 17)
            {
                _dealerHand.AddCard(_shoe.TakeNextCard());
                _logger?.Info($"Dealer hits. Dealer's hand value: {_dealerHand.HandValue}");
            }

            if (_dealerHand.IsBusted)
            {
                _logger?.Info("Dealer is busted!");
            }
            else
            {
                _logger?.Info($"Dealer stands with a hand value of {_dealerHand.HandValue}");
            }
        }

        protected void EvaluateGameOutcome()
        {
            foreach (var hand in _playerHands)
            {
                if (hand.IsBusted)
                {
                    _logger?.Info("Player hand is busted. Dealer wins.");
                    PlayerMoney -= InitialBet;
                }
                else if (_dealerHand.IsBusted)
                {
                    _logger?.Info("Dealer is busted. Player wins.");
                    PlayerMoney += InitialBet;
                }
                else if (hand.HandValue > _dealerHand.HandValue)
                {
                    _logger?.Info("Player wins with a higher hand value.");
                    PlayerMoney += InitialBet;
                }
                else if (hand.HandValue < _dealerHand.HandValue)
                {
                    _logger?.Info("Dealer wins with a higher hand value.");
                    PlayerMoney -= InitialBet;
                }
                else
                {
                    _logger?.Info("It's a tie.");
                }
            }
        }
    }
}
