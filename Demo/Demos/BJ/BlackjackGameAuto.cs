namespace Demo.Demos.BJ;

public class BlackjackGameAuto : BlackjackGameBase
{
    private readonly StrategyTable _strategyTable;

    public BlackjackGameAuto(
        Shoe shoe,
        StrategyTable strategyTable,
        ILogger? logger = null,
        BlackjackRulesOptions? rulesOptions = null)
        : base(shoe, logger, rulesOptions)
    {
        _strategyTable = strategyTable;
    }

    public async Task PlayGameAsync()
    {
        while (!Shoe.IsRedCardRaised && !Shoe.IsShoeEmpty)
        {
            await StartNewRoundAsync();
            await PlayRoundAsync();
        }
    }

    private async Task PlayRoundAsync()
    {
        while (CanTakeAction() && !GameFinished)
        {
            var dealerCardValue = DealerHand.Cards.First(card => card.IsFaceUp).Value;
            var action = _strategyTable.GetAction(CurrentPlayerHand!, dealerCardValue);

            await ExecuteActionAsync(action);
        }

        if (!GameFinished)
        {
            await EndHandAsync();
        }
    }

    private bool CanTakeAction()
    {
        return CanHit() || CanStand() || CanDoubleDown() || CanSplit() || CanSurrender();
    }

    private async Task ExecuteActionAsync(BlackjackAction action)
    {
        switch (action)
        {
            case BlackjackAction.Hit:
                await HitAsync();
                break;
            case BlackjackAction.Stand:
                await StandAsync();
                break;
            case BlackjackAction.Double:
                if (CanDoubleDown())
                {
                    await DoubleDownAsync();
                }
                else
                {
                    await HitAsync();
                }

                break;
            case BlackjackAction.Split:
                if (CanSplit())
                {
                    await SplitAsync();
                }
                else
                {
                    var dealerCardValue = DealerHand.Cards.First(card => card.IsFaceUp).Value;
                    var fallbackAction = _strategyTable.GetAction(
                        new HandValuesAdapter(CurrentPlayerHand!.HandValue, CurrentPlayerHand.IsSoft, isPair: false,
                            pairRank: HandValuesAdapter.NoPairRankIndex),
                        dealerCardValue);

                    if (fallbackAction == BlackjackAction.Split)
                    {
                        fallbackAction = BlackjackAction.Hit;
                    }

                    await ExecuteActionAsync(fallbackAction);
                }

                break;
            case BlackjackAction.Surrender:
                if (CanSurrender())
                {
                    await SurrenderAsync();
                }
                else
                {
                    await HitAsync();
                }

                break;
        }
    }

    /// <summary>
    /// Adapter that exposes the current hand values to the strategy table.
    /// </summary>
    private class HandValuesAdapter : IHandValues
    {
        public const int NoPairRankIndex = -1;

        public HandValuesAdapter(int handValue, bool isSoft, bool isPair, int pairRank)
        {
            HandValue = handValue;
            IsSoft = isSoft;
            IsPair = isPair;
            PairRank = isPair ? pairRank : NoPairRankIndex;
            IsBusted = handValue > 21;
            AllCardFacedUp = true; // All player cards are visible in the simulation
        }

        public bool AllCardFacedUp { get; }
        public int HandValue { get; }
        public bool IsBusted { get; }
        public bool IsSoft { get; }
        public bool IsPair { get; }
        public int PairRank { get; }
    }
}