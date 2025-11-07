using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Demo.Demos.BJ;

public class BlackjackGameBase
{
    public enum GameState
    {
        GameNotStarted,
        GameStarted,
        GameFinished
    }

    private const decimal InitialBet = 10m;
    private const decimal SurrenderPenalty = InitialBet / 2;
    private const int MaxSplits = 3;

    private readonly ILogger? _logger;
    public readonly Shoe Shoe;

    private int _currentPlayerHandIndex;


    // TODO Will be defined later
    public int GamesPlayedInShoe;

    protected BlackjackRulesOptions RulesOptions { get; }

    protected BlackjackGameBase(Shoe shoe, ILogger? logger = null, BlackjackRulesOptions? rulesOptions = null)
    {
        _logger = logger;
        Shoe = shoe;
        RulesOptions = rulesOptions ?? BlackjackRulesOptions.Default;

        _currentPlayerHandIndex = 0;
        CurrentGameState = GameState.GameNotStarted;

        PlayerHands = new List<PlayerHand>();
        DealerHand = new DealerHand(new List<Card>());
    }

    public List<PlayerHand> PlayerHands { get; private set; }
    public PlayerHand? CurrentPlayerHand => PlayerHands.Count > 0 ? PlayerHands[_currentPlayerHandIndex] : null;
    public DealerHand DealerHand { get; private set; }
    public decimal PlayerBalance { get; private set; } = 1000;
    public GameState CurrentGameState { get; private set; }

    public bool GameFinished => CurrentGameState == GameState.GameFinished;

    public event Func<bool, Task>? GameStateChanged;

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

    private Card DealCardToHand(PlayerHand hand)
    {
        var card = Shoe.TakeNextCard();
        hand.AddCard(card);

        var handIndex = PlayerHands.IndexOf(hand);
        _logger?.Info(
            $"Card {card} dealt to player hand {handIndex}. Hand value: {hand.HandValue}");

        return card;
    }

    protected void ResetGameVariables()
    {
        _currentPlayerHandIndex = 0;
        CurrentGameState = GameState.GameNotStarted;

        PlayerHands.Clear();
        PlayerHands.Add(new PlayerHand(DealPlayerInitialCards(), InitialBet));

        DealerHand._cards.Clear();
        DealerHand._cards.AddRange(DealDealerInitialCards());
    }

    private void LogGameState(string message)
    {
        _logger?.Info(message);
        _logger?.Info("Logging game state...");
        _logger?.Info(
            $"Dealer's hand ({DealerHand.HandValue}): {string.Join(", ", DealerHand.Cards.Select(c => c.ToString()))}");

        for (var i = 0; i < PlayerHands.Count; i++)
        {
            var hand = PlayerHands[i];
            _logger?.Info(
                $"Player hand {i}: ({hand.HandValue}) {string.Join(", ", hand.Cards.Select(c => c.ToString()))}");
        }

        _logger?.Info($"CanHit: {CanHit()}");
        _logger?.Info($"CanStand: {CanStand()}");
        _logger?.Info($"CanDoubleDown: {CanDoubleDown()}");
        _logger?.Info($"CanSplit: {CanSplit()}");
        _logger?.Info($"CanSurrender: {CanSurrender()}");
        _logger?.Info($"CanStartNewRound: {CanStartNewRound()}");
    }

    // Can methods
    public bool CanStartNewRound() => CurrentGameState != GameState.GameStarted;

    public bool CanHit()
    {
        if (CurrentGameState != GameState.GameStarted)
        {
            return false;
        }

        if (CurrentPlayerHand is not { IsCompleted: false } hand)
        {
            return false;
        }

        return hand.HandValue < 21;
    }

    public bool CanStand()
    {
        return CurrentGameState == GameState.GameStarted && CurrentPlayerHand is { IsCompleted: false };
    }

    public bool CanDoubleDown()
    {
        return CurrentGameState == GameState.GameStarted &&
               CurrentPlayerHand is { IsCompleted: false } hand &&
               hand.Cards.Count == 2;
    }

    public bool CanSplit()
    {
        if (CurrentGameState != GameState.GameStarted)
        {
            return false;
        }

        if (CurrentPlayerHand is not { IsCompleted: false } hand)
        {
            return false;
        }

        return hand.Cards.Count == 2 &&
               hand.Cards[0].Rank == hand.Cards[1].Rank &&
               PlayerHands.Count < MaxSplits;
    }

    public bool CanSurrender()
    {
        return CurrentGameState == GameState.GameStarted &&
               CurrentPlayerHand is { IsCompleted: false } hand &&
               hand.Cards.Count == 2;
    }

    // Action methods
    public async Task StartNewRoundAsync()
    {
        if (!CanStartNewRound())
            throw new InvalidOperationException("Cannot start a new round at this time.");

        PlayerBalance -= InitialBet;
        PlayerHands = new List<PlayerHand> { new PlayerHand(DealPlayerInitialCards(), InitialBet) };
        _currentPlayerHandIndex = 0;
        DealerHand = new DealerHand(DealDealerInitialCards());
        CurrentGameState = GameState.GameStarted;

        if (CurrentPlayerHand!.HandValue == 21)
        {
            _logger?.Info("Player has a Blackjack (21) from the initial deal.");
            await EndHandAsync();
        }

        await OnGameStateChangedAsync(false);
    }

    // Updated method calls
    public async Task HitAsync()
    {
        LogGameState("Action selected: Hit");

        if (!CanHit()) throw new InvalidOperationException("Cannot hit at this time.");

        var hand = CurrentPlayerHand!;
        var card = DealCardToHand(hand);
        _logger?.Info($"Player hits and receives card: {card}. Hand value: {hand.HandValue}");

        if (hand.HandValue == 21 || hand.IsBusted)
        {
            _logger?.Info($"Player hand {_currentPlayerHandIndex} is either 21 or busted.");
            await EndHandAsync();
        }

        await OnGameStateChangedAsync(false);
    }

    public async Task StandAsync()
    {
        if (!CanStand() || CurrentPlayerHand == null)
            throw new InvalidOperationException("Cannot stand at this time.");

        LogGameState($"Action selected: Stand with hand value: {CurrentPlayerHand.HandValue}");

        await EndHandAsync();
        await OnGameStateChangedAsync(false);
    }

    public async Task DoubleDownAsync()
    {
        LogGameState("Action selected: Double Down");
        if (!CanDoubleDown()) throw new InvalidOperationException("Cannot double down at this time.");

        PlayerBalance -= InitialBet;
        CurrentPlayerHand!.DoubleBet();
        _logger?.Info("Player doubles down. Bet is doubled.");

        var hand = CurrentPlayerHand!;
        var card = DealCardToHand(hand);
        _logger?.Info($"Player receives one card: {card}. Hand value: {hand.HandValue}");

        // Ends the turn automatically after Double Down
        await StandAsync();
        await OnGameStateChangedAsync(false);
    }

    public async Task SplitAsync()
    {
        LogGameState("Action selected: Split");
        if (!CanSplit() || CurrentPlayerHand == null)
            throw new InvalidOperationException("Cannot split at this time.");

        var currentHand = CurrentPlayerHand;
        var currentIndex = _currentPlayerHandIndex;
        var newHand = currentHand.Split();
        PlayerBalance -= InitialBet;
        PlayerHands.Insert(currentIndex + 1, newHand);
        _logger?.Info("Player splits the hand. Two hands will now play sequentially.");

        var firstSplitCard = DealCardToHand(currentHand);
        _logger?.Info(
            $"Player hand {currentIndex} receives split card: {firstSplitCard}. Hand value: {currentHand.HandValue}");

        var secondSplitCard = DealCardToHand(newHand);
        var newHandIndex = currentIndex + 1;
        _logger?.Info(
            $"Player hand {newHandIndex} receives split card: {secondSplitCard}. Hand value: {newHand.HandValue}");

        var isSplitAces = currentHand.WasSplitAce && newHand.WasSplitAce;

        if (isSplitAces && !RulesOptions.CanHitAfterSplitAces)
        {
            currentHand.MarkCompleted();
            newHand.MarkCompleted();
            _logger?.Info("Split aces are completed immediately per table rules.");
            await EndHandAsync();
        }

        await OnGameStateChangedAsync(false);
    }

    public async Task SurrenderAsync()
    {
        LogGameState("Action selected: Surrender");
        if (!CanSurrender()) throw new InvalidOperationException("Cannot surrender at this time.");

        PlayerBalance -= SurrenderPenalty;
        CurrentGameState = GameState.GameFinished;
        _logger?.Info(
            $"Player surrenders. Lost half of the bet: {SurrenderPenalty}. Remaining money: {PlayerBalance}");
        await OnGameStateChangedAsync(false);
    }

    protected async Task EndHandAsync()
    {
        var hand = CurrentPlayerHand;
        if (hand == null)
        {
            return;
        }

        _logger?.Info("Player's turn ends for hand index: " + _currentPlayerHandIndex);

        if (hand.IsBusted)
        {
            _logger?.Info("Hand is busted.");
        }

        hand.MarkCompleted();

        var nextHandIndex = GetNextActiveHandIndex(_currentPlayerHandIndex + 1);
        if (nextHandIndex.HasValue)
        {
            _currentPlayerHandIndex = nextHandIndex.Value;
            _logger?.Info($"Moving to next hand. Current hand index: {_currentPlayerHandIndex}");
            return;
        }

        if (PlayerHands.All(playerHand => playerHand.IsCompleted))
        {
            _logger?.Info("All hands have been played. Dealer starts playing.");
            await DealerPlayAsync();
            CurrentGameState = GameState.GameFinished;
        }
    }

    private int? GetNextActiveHandIndex(int startIndex)
    {
        for (var i = startIndex; i < PlayerHands.Count; i++)
        {
            if (!PlayerHands[i].IsCompleted)
            {
                return i;
            }
        }

        return null;
    }

    private async Task DealerPlayAsync()
    {
        if (CurrentGameState == GameState.GameFinished)
        {
            _logger?.Info("Game is already finished. Dealer does not play.");
            return;
        }

        DealerHand.FlipSecondCard();
        await OnGameStateChangedAsync(true);

        while (DealerHand.HandValue < 17)
        {
            var card = Shoe.TakeNextCard() with { IsFaceUp = false };
            DealerHand.AddCard(card);
            await OnGameStateChangedAsync(true);
            DealerHand.FlipCard(card);
            await OnGameStateChangedAsync(true);
            _logger?.Info($"Dealer takes card: {card}");
        }

        _logger?.Info("Dealer finishes playing.");
        CheckWinConditions();
    }

    private void CheckWinConditions()
    {
        foreach (var hand in PlayerHands)
        {
            if (hand.IsBusted)
            {
                SetOutcomeAndLog(hand, -hand.Bet, "Player loses this hand due to bust.");
                continue;
            }

            if (DealerHand.IsBusted)
            {
                SetOutcomeAndLog(hand, hand.Bet, "Dealer busts. Player wins this hand.");
                PlayerBalance += hand.Bet * 2;
                continue;
            }

            if (hand.HandValue > DealerHand.HandValue)
            {
                SetOutcomeAndLog(hand, hand.Bet, "Player wins this hand.");
                PlayerBalance += hand.Bet * 2;
                continue;
            }

            if (hand.HandValue < DealerHand.HandValue)
            {
                SetOutcomeAndLog(hand, -hand.Bet, "Dealer wins this hand.");
                continue;
            }

            // Push case
            SetOutcomeAndLog(hand, 0, "Push. No money lost or gained.");
            PlayerBalance += hand.Bet;
        }

        _logger?.Info($"Game over. Player money: {PlayerBalance}");
    }

    private void SetOutcomeAndLog(PlayerHand hand, decimal money, string message)
    {
        hand.SetOutcome(new HandOutcome(money, message));
        _logger?.Info($"{hand.Outcome}");
    }


    private async Task OnGameStateChangedAsync(bool isDealerAction)
    {
        if (GameStateChanged != null)
        {
            await GameStateChanged(isDealerAction);
        }
    }
}