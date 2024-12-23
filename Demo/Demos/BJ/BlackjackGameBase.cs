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

    protected BlackjackGameBase(Shoe shoe, ILogger? logger)
    {
        _logger = logger;
        Shoe = shoe;

        _currentPlayerHandIndex = 0;
        CurrentGameState = GameState.GameNotStarted;
    }

    public List<PlayerHand> PlayerHands { get; private set; }
    public PlayerHand? CurrentPlayerHand => PlayerHands[_currentPlayerHandIndex];
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

    public bool CanHit() => CurrentGameState == GameState.GameStarted && CurrentPlayerHand?.HandValue < 21;

    public bool CanStand() => CurrentGameState == GameState.GameStarted;

    public bool CanDoubleDown() => CurrentGameState == GameState.GameStarted && CurrentPlayerHand?.Cards.Count == 2;

    public bool CanSplit() =>
        CurrentGameState == GameState.GameStarted &&
        CurrentPlayerHand?.Cards.Count == 2 &&
        CurrentPlayerHand.Cards[0].Rank == CurrentPlayerHand.Cards[1].Rank &&
        PlayerHands.Count <= MaxSplits;

    public bool CanSurrender() => CurrentGameState == GameState.GameStarted && CurrentPlayerHand?.Cards.Count == 2;

    // Action methods
    public async Task StartNewRoundAsync()
    {
        if (!CanStartNewRound())
            throw new InvalidOperationException("Cannot start a new round at this time.");

        PlayerBalance -= InitialBet;
        PlayerHands = [new PlayerHand(DealPlayerInitialCards(), InitialBet)];
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

        var card = Shoe.TakeNextCard();
        CurrentPlayerHand!.AddCard(card);
        _logger?.Info($"Player hits and receives card: {card}. Hand value: {CurrentPlayerHand.HandValue}");

        if (CurrentPlayerHand.HandValue == 21 || CurrentPlayerHand.IsBusted)
        {
            _logger?.Info($"Player hand {_currentPlayerHandIndex} is either 21 or busted.");
            await EndHandAsync();
        }

        await OnGameStateChangedAsync(false);
    }

    public async Task StandAsync()
    {
        LogGameState($"Action selected: Stand with hand value: {CurrentPlayerHand!.HandValue}");
        if (!CanStand()) throw new InvalidOperationException("Cannot stand at this time.");

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

        var card = Shoe.TakeNextCard();
        CurrentPlayerHand.AddCard(card);
        _logger?.Info($"Player receives one card: {card}. Hand value: {CurrentPlayerHand.HandValue}");

        // Ends the turn automatically after Double Down
        await StandAsync();
        await OnGameStateChangedAsync(false);
    }

    public async Task SplitAsync()
    {
        LogGameState("Action selected: Split");
        if (!CanSplit() || CurrentPlayerHand == null) throw new InvalidOperationException("Cannot split at this time.");

        var newHand = CurrentPlayerHand.Split();
        PlayerBalance -= InitialBet;
        PlayerHands.Add(newHand);
        _logger?.Info("Player splits the hand. Two new hands created.");
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
        _logger?.Info("Player's turn ends for hand index: " + _currentPlayerHandIndex);

        if (CurrentPlayerHand!.IsBusted) _logger?.Info("Hand is busted.");

        if (_currentPlayerHandIndex < PlayerHands.Count - 1)
        {
            _currentPlayerHandIndex++;
            _logger?.Info($"Moving to next hand. Current hand index: {_currentPlayerHandIndex}");
        }
        else
        {
            _logger?.Info("All hands have been played. Dealer starts playing.");
            await DealerPlayAsync();
            CurrentGameState = GameState.GameFinished;
        }
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