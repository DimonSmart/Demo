namespace Demo.Demos.BJ
{
    public class BlackjackGameAuto : BlackjackGameBase
    {
        private readonly StrategyTable _strategyTable;

        public BlackjackGameAuto(Shoe shoe, StrategyTable strategyTable, ILogger? logger = null) : base(shoe, logger)
        {
            _strategyTable = strategyTable;
        }

        public async Task PlayGameAsync()
        {
            await StartNewRoundAsync();

            while (!Shoe.IsRedCardRaised && !Shoe.IsShoeEmpty)
            {
                await PlayRoundAsync();
                ResetGameVariables();
            }
        }

        private async Task PlayRoundAsync()
        {
            if (CurrentGameState != GameState.GameStarted)
                throw new InvalidOperationException("The game is not in a valid state to start a new round.");

            while (CanTakeAction() && GameFinished)
            {
                var dealerCardValue = DealerHand.Cards.First(card => card.IsFaceUp).Value;
                var action = _strategyTable.GetAction(CurrentPlayerHand, dealerCardValue);

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
                        await HitAsync();
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
    }
}
