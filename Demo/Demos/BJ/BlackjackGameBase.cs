using Demo.Components;

namespace Demo.Demos.BJ;

public class BlackjackGameBase
{
    public readonly List<PlayerHand> PlayerHands;
    public PlayerHand CurrentPlayerHand => PlayerHands[_currentPlayerHandIndex];
    public readonly Hand DealerHand;
    public decimal PlayerBalance;
    public readonly Shoe Shoe;

    private int _currentPlayerHandIndex;
    public bool GameFinished;

    // TODO Will be defined later
    public int GamesPlayedInShoe;

    private const decimal InitialBet = 10m;
    private const decimal SurrenderPenalty = InitialBet / 2;
    private const int MaxSplits = 3;

    private readonly ILogger? _logger;

    protected BlackjackGameBase(Shoe shoe, ILogger? logger)
    {
        _logger = logger;
        Shoe = shoe;

        _currentPlayerHandIndex = 0;
        GameFinished = false;

        PlayerHands = [new PlayerHand(DealPlayerInitialCards(), InitialBet)];
        DealerHand = new Hand(DealDealerInitialCards());

        if (CurrentPlayerHand.HandValue != 21) return;

        _logger?.Info("Player has a Blackjack (21) from the initial deal.");
        EndHand();
    }

    private List<Card> DealDealerInitialCards()
    {
        var firstCard = Shoe.TakeNextCard();
        var secondCard = Shoe.TakeNextCard() with { IsFaceUp = false };
        var initialDealerCards = new List<Card> { firstCard, secondCard };
        _logger?.Info($"Initial dealer card dealt: {firstCard}");
        return initialDealerCards;
    }

    private List<Card> DealPlayerInitialCards()
    {
        var firstCard = Shoe.TakeNextCard();
        var secondCard = Shoe.TakeNextCard();
        var initialPlayerCards = new List<Card> { firstCard, secondCard };
        _logger?.Info($"Initial player cards dealt: {firstCard}, {secondCard}");
        return initialPlayerCards;
    }

    protected void ResetGameVariables()
    {
        _currentPlayerHandIndex = 0;
        GameFinished = false;

        PlayerHands.Clear();
        PlayerHands.Add(new PlayerHand(DealPlayerInitialCards(), InitialBet));

        DealerHand._cards.Clear();
        DealerHand._cards.AddRange(DealDealerInitialCards());
    }

    private void LogGameState()
    {
        _logger?.Info("Logging game state...");

        _logger?.Info($"Dealer's hand ({DealerHand.HandValue}): {string.Join(", ", DealerHand.Cards.Select(c => c.ToString()))}");

        for (var i = 0; i < PlayerHands.Count; i++)
        {
            var hand = PlayerHands[i];
            _logger?.Info($"Player hand {i}: ({hand.HandValue}) {string.Join(", ", hand.Cards.Select(c => c.ToString()))}");
        }

        _logger?.Info($"CanHit: {CanHit()}");
        _logger?.Info($"CanStand: {CanStand()}");
        _logger?.Info($"CanDoubleDown: {CanDoubleDown()}");
        _logger?.Info($"CanSplit: {CanSplit()}");
        _logger?.Info($"CanSurrender: {CanSurrender()}");
    }

    // Can methods
    public bool CanHit() => !GameFinished && CurrentPlayerHand.HandValue < 21;

    public bool CanStand() => !GameFinished;

    public bool CanDoubleDown() => !GameFinished && CurrentPlayerHand.Cards.Count == 2;

    public bool CanSplit() =>
        !GameFinished &&
        CurrentPlayerHand.Cards.Count == 2 &&
        CurrentPlayerHand.Cards[0].Rank == CurrentPlayerHand.Cards[1].Rank &&
        PlayerHands.Count <= MaxSplits;

    public bool CanSurrender() => !GameFinished && CurrentPlayerHand.Cards.Count == 2;

    // Action methods
    public void Hit()
    {
        LogGameState();
        _logger?.Info("Action selected: Hit");

        if (!CanHit())
        {
            throw new InvalidOperationException("Cannot hit at this time.");
        }

        var card = Shoe.TakeNextCard();
        CurrentPlayerHand.AddCard(card);
        _logger?.Info($"Player hits and receives card: {card}. Hand value: {CurrentPlayerHand.HandValue}");

        if (CurrentPlayerHand.HandValue == 21 || CurrentPlayerHand.IsBusted)
        {
            _logger?.Info($"Player hand {_currentPlayerHandIndex} is either 21 or busted.");
            EndHand();
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
        EndHand();
    }

    public void DoubleDown()
    {
        LogGameState();
        _logger?.Info("Action selected: Double Down");

        if (!CanDoubleDown())
        {
            throw new InvalidOperationException("Cannot double down at this time.");
        }

        PlayerBalance -= InitialBet;
        CurrentPlayerHand.Bet *= 2;
        _logger?.Info("Player doubles down. Bet is doubled.");

        var card = Shoe.TakeNextCard();
        CurrentPlayerHand.AddCard(card);
        _logger?.Info($"Player receives one card: {card}. Hand value: {CurrentPlayerHand.HandValue}");

        // Ends the turn automatically after Double Down
        Stand(); 
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
        PlayerBalance -= InitialBet;
        PlayerHands.Add(newHand);
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

        PlayerBalance -= SurrenderPenalty;
        GameFinished = true;
        _logger?.Info($"Player surrenders. Lost half of the bet: {SurrenderPenalty}. Remaining money: {PlayerBalance}");
    }

    protected void EndHand()
    {
        _logger?.Info("Player's turn ends for hand index: " + _currentPlayerHandIndex);

        if (CurrentPlayerHand.IsBusted)
        {
            _logger?.Info("Hand is busted.");
        }

        if (_currentPlayerHandIndex < PlayerHands.Count - 1)
        {
            _currentPlayerHandIndex++;
            _logger?.Info($"Moving to next hand. Current hand index: {_currentPlayerHandIndex}");
        }
        else
        {
            GameFinished = true;
            _logger?.Info("All hands have been played. Dealer starts playing.");
            DealerPlay();
        }
    }

    private void DealerPlay()
    {
        if (GameFinished)
        {
            _logger?.Info("Game is already finished. Dealer does not play.");
            return;
        }

        while (DealerHand.HandValue < 17)
        {
            var card = Shoe.TakeNextCard();
            DealerHand.AddCard(card);
            _logger?.Info($"Dealer takes card: {card}");
        }

        _logger?.Info("Dealer finishes playing.");
        CheckWinConditions();
    }

    private void CheckWinConditions()
    {
        for (var i = 0; i < PlayerHands.Count; i++)
        {
            var hand = PlayerHands[i];

            if (hand.IsBusted)
            {
                _logger?.Info("Player loses this hand due to bust.");
                PlayerBalance -= hand.Bet;
            }
            else if (DealerHand.IsBusted)
            {
                _logger?.Info("Dealer busts. Player wins this hand.");
                PlayerBalance += hand.Bet;
            }
            else
            {
                if (hand.HandValue > DealerHand.HandValue)
                {
                    _logger?.Info("Player wins this hand.");
                    PlayerBalance += hand.Bet;
                }
                else if (hand.HandValue < DealerHand.HandValue)
                {
                    _logger?.Info("Dealer wins this hand.");
                    PlayerBalance -= hand.Bet;
                }
                else
                {
                    _logger?.Info("Push. No money lost or gained.");
                }
            }
        }

        _logger?.Info($"Game over. Player money: {PlayerBalance}");
    }
}
