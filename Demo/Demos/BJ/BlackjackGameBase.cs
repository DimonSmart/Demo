namespace Demo.Demos.BJ;

public class BlackjackGameBase
{
    protected readonly Shoe _shoe;
    protected readonly List<PlayerHand> _playerHands;
    protected int _currentPlayerHandIndex;
    protected readonly Hand _dealerHand;
    protected bool _gameFinished;
    private decimal _playerMoney;
    private const decimal InitialBet = 10m;
    private const decimal SurrenderPenalty = InitialBet / 2;
    private const int MaxSplits = 3;

    private readonly ILogger? _logger;

    protected BlackjackGameBase(Shoe shoe, ILogger? logger)
    {
        _logger = logger;
        _shoe = shoe;

        _currentPlayerHandIndex = 0;
        _gameFinished = false;

        _playerHands = new List<PlayerHand> { new PlayerHand(DealPlayerInitialCards(), InitialBet) };
        _dealerHand = new Hand(DealDealerInitialCards());

        // Check if the player has a Blackjack (21) from the initial deal
        if (CurrentPlayerHand.HandValue == 21)
        {
            _logger?.Info("Player has a Blackjack (21) from the initial deal.");
            EndPlayerTurn();
        }
    }

    protected PlayerHand CurrentPlayerHand => _playerHands[_currentPlayerHandIndex];

    private List<Card> DealDealerInitialCards()
    {
        var firstCard = _shoe.TakeNextCard();
        var secondCard = _shoe.TakeNextCard() with { IsFaceUp = false };
        var initialDealerCards = new List<Card> { firstCard, secondCard };
        _logger?.Info($"Initial dealer card dealt: {firstCard}");
        return initialDealerCards;
    }

    private List<Card> DealPlayerInitialCards()
    {
        var firstCard = _shoe.TakeNextCard();
        var secondCard = _shoe.TakeNextCard();
        var initialPlayerCards = new List<Card> { firstCard, secondCard };
        _logger?.Info($"Initial player cards dealt: {firstCard}, {secondCard}");
        return initialPlayerCards;
    }

    protected void ResetGameVariables()
    {
        _currentPlayerHandIndex = 0;
        _gameFinished = false;

        _playerHands.Clear();
        _playerHands.Add(new PlayerHand(DealPlayerInitialCards(), InitialBet));

        _dealerHand._cards.Clear();
        _dealerHand._cards.AddRange(DealDealerInitialCards());
    }

    private void LogGameState()
    {
        _logger?.Info("Logging game state...");

        // Log dealer's hand
        _logger?.Info($"Dealer's hand ({_dealerHand.HandValue}): {string.Join(", ", _dealerHand.Cards.Select(c => c.ToString()))}");

        // Log each player's hand
        for (int i = 0; i < _playerHands.Count; i++)
        {
            var hand = _playerHands[i];
            _logger?.Info($"Player hand {i}: ({hand.HandValue}) {string.Join(", ", hand.Cards.Select(c => c.ToString()))}");
        }

        // Log "Can" conditions
        _logger?.Info($"CanHit: {CanHit()}");
        _logger?.Info($"CanStand: {CanStand()}");
        _logger?.Info($"CanDoubleDown: {CanDoubleDown()}");
        _logger?.Info($"CanSplit: {CanSplit()}");
        _logger?.Info($"CanSurrender: {CanSurrender()}");
    }

    // Can methods
    public bool CanHit() => !_gameFinished && CurrentPlayerHand.HandValue < 21;

    public bool CanStand() => !_gameFinished;

    public bool CanDoubleDown() => !_gameFinished && CurrentPlayerHand.Cards.Count == 2;

    public bool CanSplit() =>
        !_gameFinished && CurrentPlayerHand.Cards.Count == 2 && CurrentPlayerHand.Cards[0].Rank == CurrentPlayerHand.Cards[1].Rank && _playerHands.Count <= MaxSplits;

    public bool CanSurrender() => !_gameFinished && CurrentPlayerHand.Cards.Count == 2;

    // Action methods
    public void Hit()
    {
        LogGameState();
        _logger?.Info("Action selected: Hit");

        if (!CanHit())
        {
            throw new InvalidOperationException("Cannot hit at this time.");
        }

        var card = _shoe.TakeNextCard();
        CurrentPlayerHand.AddCard(card);
        _logger?.Info($"Player hits and receives card: {card}. Hand value: {CurrentPlayerHand.HandValue}");

        if (CurrentPlayerHand.HandValue == 21)
        {
            _logger?.Info($"Player hand {_currentPlayerHandIndex} reaches 21.");
            if (_currentPlayerHandIndex < _playerHands.Count - 1)
            {
                _currentPlayerHandIndex++;
            }
            else
            {
                EndPlayerTurn();
            }
        }
        else if (CurrentPlayerHand.IsBusted)
        {
            _logger?.Info($"Player hand {_currentPlayerHandIndex} busts.");
            if (_currentPlayerHandIndex < _playerHands.Count - 1)
            {
                _currentPlayerHandIndex++;
            }
            else
            {
                EndPlayerTurn();
            }
        }
    }

    public void Stand()
    {
        LogGameState();
        _logger?.Info("Action selected: Stand");

        if (!CanStand())
        {
            throw new InvalidOperationException("Cannot stand at this time.");
        }

        _logger?.Info($"Player stands with hand value: {CurrentPlayerHand.HandValue}");

        if (_currentPlayerHandIndex < _playerHands.Count - 1)
        {
            _currentPlayerHandIndex++;
        }
        else
        {
            EndPlayerTurn();
        }
    }

    public void DoubleDown()
    {
        LogGameState();
        _logger?.Info("Action selected: Double Down");

        if (!CanDoubleDown())
        {
            throw new InvalidOperationException("Cannot double down at this time.");
        }

        _playerMoney -= InitialBet;
        CurrentPlayerHand.Bet *= 2;
        _logger?.Info("Player doubles down. Bet is doubled.");

        var card = _shoe.TakeNextCard();
        CurrentPlayerHand.AddCard(card);
        _logger?.Info($"Player receives one card: {card}. Hand value: {CurrentPlayerHand.HandValue}");

        Stand(); // Ends the turn automatically after Double Down
    }

    public void Split()
    {
        LogGameState();
        _logger?.Info("Action selected: Split");

        if (!CanSplit())
        {
            throw new InvalidOperationException("Cannot split at this time.");
        }

        var newHand = CurrentPlayerHand.Split();
        newHand.Bet = InitialBet;
        _playerMoney -= InitialBet;
        _playerHands.Add(newHand);
        _logger?.Info("Player splits the hand. Two new hands created.");
    }

    public void Surrender()
    {
        LogGameState();
        _logger?.Info("Action selected: Surrender");

        if (!CanSurrender())
        {
            throw new InvalidOperationException("Cannot surrender at this time.");
        }

        _playerMoney -= SurrenderPenalty;
        _gameFinished = true;
        _logger?.Info($"Player surrenders. Lost half of the bet: {SurrenderPenalty}. Remaining money: {_playerMoney}");
    }

    protected void EndPlayerTurn()
    {
        _gameFinished = true;
        _logger?.Info("Player's turn ends. Dealer starts playing.");
        DealerPlay();
    }

    private void DealerPlay()
    {
        while (_dealerHand.HandValue < 17)
        {
            _dealerHand.AddCard(_shoe.TakeNextCard());
        }

        _logger?.Info($"Dealer finishes playing with hand value: {_dealerHand.HandValue}");
        CheckWinConditions();
    }

    private void CheckWinConditions()
    {
        for (int i = 0; i < _playerHands.Count; i++)
        {
            var hand = _playerHands[i];

            if (hand.IsBusted)
            {
                _logger?.Info("Player loses this hand due to bust.");
                _playerMoney -= hand.Bet;
            }
            else if (_dealerHand.IsBusted)
            {
                _logger?.Info("Dealer busts. Player wins this hand.");
                _playerMoney += hand.Bet;
            }
            else
            {
                if (hand.HandValue > _dealerHand.HandValue)
                {
                    _logger?.Info("Player wins this hand.");
                    _playerMoney += hand.Bet;
                }
                else if (hand.HandValue < _dealerHand.HandValue)
                {
                    _logger?.Info("Dealer wins this hand.");
                    _playerMoney -= hand.Bet;
                }
                else
                {
                    _logger?.Info("Push. No money lost or gained.");
                }
            }
        }

        _logger?.Info($"Game over. Player money: {_playerMoney}");
    }
}
