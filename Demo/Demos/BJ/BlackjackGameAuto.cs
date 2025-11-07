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
                    // If we can not "double" we just hit
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
                    // Если сплит недоступен (например, достигнут лимит),
                    // играем руку как обычную пару - получаем рекомендацию для несплитованной руки
                    var dealerCardValue = DealerHand.Cards.First(card => card.IsFaceUp).Value;
                    var fallbackAction = _strategyTable.GetAction(
                        new HandValuesAdapter(CurrentPlayerHand!.HandValue, CurrentPlayerHand.IsSoft, isPair: false, pairRank: 0),
                        dealerCardValue);
                    
                    // Выполняем альтернативное действие, но избегаем бесконечной рекурсии
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
                    // Если сдача недоступна, берём карту
                    await HitAsync();
                }

                break;
        }
    }

    /// <summary>
    /// Адаптер для передачи значений руки в таблицу стратегии
    /// </summary>
    private class HandValuesAdapter : IHandValues
    {
        public HandValuesAdapter(int handValue, bool isSoft, bool isPair, int pairRank)
        {
            HandValue = handValue;
            IsSoft = isSoft;
            IsPair = isPair;
            PairRank = pairRank;
            IsBusted = handValue > 21;
            AllCardFacedUp = true; // Для игрока все карты всегда открыты
        }

        public bool AllCardFacedUp { get; }
        public int HandValue { get; }
        public bool IsBusted { get; }
        public bool IsSoft { get; }
        public bool IsPair { get; }
        public int PairRank { get; }
    }
}