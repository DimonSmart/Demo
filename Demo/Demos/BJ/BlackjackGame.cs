namespace Demo.Demos.BJ
{
    public class BlackjackGame
    {
        private const decimal InitialBet = 10m;
        private const decimal SurrenderPenalty = InitialBet / 2;

        private List<Hand> _playerHands;
        private Hand _dealerHand;

        private readonly Shoe _shoe;
        private readonly StrategyTable _strategyTable;
        private readonly ILogger? _logger;

        public int TotalGamesCount { get; private set; }
        public decimal PlayerMoney { get; private set; }

        public BlackjackGame(Shoe shoe, StrategyTable strategyTable, ILogger? logger = null)
        {
            _shoe = shoe;
            _strategyTable = strategyTable;
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

        private void PlaySingleGame()
        {
            _logger?.Info("Dealing initial cards to player and dealer.");
            _playerHands = new List<Hand> { new Hand(DealInitialCards()) };
            _dealerHand = new Hand(DealInitialCards());

            foreach (var hand in _playerHands.ToArray())
            {
                PlayPlayerHand(hand);
            }

            if (_playerHands.Count > 0 && !_playerHands[0].IsBusted)
            {
                _logger?.Info("Playing dealer's hand.");
                PlayDealerHand();
            }

            EvaluateGameOutcome();
        }

        private List<Card> DealInitialCards()
        {
            var initialCards = new List<Card> { _shoe.TakeNextCard(), _shoe.TakeNextCard() };
            _logger?.Info($"Initial cards dealt: {string.Join(", ", initialCards)}");
            return initialCards;
        }

        private void PlayPlayerHand(Hand hand)
        {
            var turnOver = false;

            while (!turnOver && !hand.IsBusted)
            {
                var dealerCardValue = _dealerHand.Cards[0].Value;
                var action = _strategyTable.GetAction(hand, dealerCardValue);

                _logger?.Info($"Player hand value: {hand.HandValue}, Dealer visible card: {dealerCardValue}, Action: {action}");

                switch (action)
                {
                    case BlackjackAction.Hit:
                        var newCard = _shoe.TakeNextCard();
                        hand.AddCard(newCard);
                        _logger?.Info($"Player hits and receives: {newCard}. New hand value: {hand.HandValue}");
                        break;

                    case BlackjackAction.Stand:
                        _logger?.Info("Player stands.");
                        turnOver = true;
                        break;

                    case BlackjackAction.Double:
                        PlayerMoney -= InitialBet;
                        var doubleCard = _shoe.TakeNextCard();
                        hand.AddCard(doubleCard);
                        _logger?.Info($"Player doubles and receives: {doubleCard}. New hand value: {hand.HandValue}");
                        turnOver = true;
                        break;

                    case BlackjackAction.Split:
                        if (hand.IsPair)
                        {
                            var newHand1 = new Hand(new List<Card> { hand.Cards[0], _shoe.TakeNextCard() });
                            var newHand2 = new Hand(new List<Card> { hand.Cards[1], _shoe.TakeNextCard() });

                            _playerHands.Remove(hand);
                            _playerHands.Add(newHand1);
                            _playerHands.Add(newHand2);

                            _logger?.Info($"Player splits into two hands: [{string.Join(", ", newHand1.Cards)}], [{string.Join(", ", newHand2.Cards)}]");
                        }
                        turnOver = true;
                        break;

                    case BlackjackAction.Surrender:
                        PlayerMoney -= SurrenderPenalty;
                        _playerHands.Remove(hand);
                        _logger?.Info("Player surrenders. Half bet lost.");
                        turnOver = true;
                        break;
                }
            }

            if (hand.IsBusted)
            {
                _logger?.Info($"Player busts with hand value: {hand.HandValue}");
            }
        }

        private void PlayDealerHand()
        {
            _logger?.Info($"Dealer starts with: {string.Join(", ", _dealerHand.Cards)}");

            while (_dealerHand.HandValue < 17)
            {
                var newCard = _shoe.TakeNextCard();
                _dealerHand.AddCard(newCard);
                _logger?.Info($"Dealer draws: {newCard}. New hand value: {_dealerHand.HandValue}");
            }

            if (_dealerHand.IsBusted)
            {
                _logger?.Info("Dealer busts.");
            }
        }

        private void EvaluateGameOutcome()
        {
            foreach (var hand in _playerHands)
            {
                if (hand.IsBusted)
                {
                    PlayerMoney -= InitialBet;
                    _logger?.Info($"Player loses. Player's busted hand value: {hand.HandValue}");
                    continue;
                }

                if (_dealerHand.IsBusted)
                {
                    PlayerMoney += InitialBet;
                    _logger?.Info($"Player wins. Dealer busted.");
                    continue;
                }

                if (hand.HandValue == _dealerHand.HandValue)
                {
                    _logger?.Info("Push. Player and dealer have the same hand value.");
                }
                else if (hand.HandValue > _dealerHand.HandValue)
                {
                    PlayerMoney += InitialBet;
                    _logger?.Info($"Player wins with hand value: {hand.HandValue} against dealer's hand value: {_dealerHand.HandValue}");
                }
                else
                {
                    PlayerMoney -= InitialBet;
                    _logger?.Info($"Player loses with hand value: {hand.HandValue} against dealer's hand value: {_dealerHand.HandValue}");
                }
            }
        }
    }
}