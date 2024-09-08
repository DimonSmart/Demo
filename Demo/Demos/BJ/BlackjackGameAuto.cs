namespace Demo.Demos.BJ
{
    public class BlackjackGameAuto : BlackjackGameBase
    {
        private readonly StrategyTable _strategyTable;

        public BlackjackGameAuto(Shoe shoe, StrategyTable strategyTable, ILogger? logger = null) : base(shoe, logger)
        {
            _strategyTable = strategyTable;
        }

        public void PlayGame()
        {
            while (!Shoe.IsRedCardRaised && !Shoe.IsShoeEmpty)
            {
                PlayRound();
                ResetGameVariables();
            }
        }

        private void PlayRound()
        {
            while (CanTakeAction() && GameFinished)
            {
                var dealerCardValue = DealerHand.Cards.First(card => card.IsFaceUp).Value;
                var action = _strategyTable.GetAction(CurrentPlayerHand, dealerCardValue);

                ExecuteAction(action);
            }

            if (!GameFinished)
            {
                EndHand();
            }
        }

        private bool CanTakeAction()
        {
            return CanHit() || CanStand() || CanDoubleDown() || CanSplit() || CanSurrender();
        }

        private void ExecuteAction(BlackjackAction action)
        {
            switch (action)
            {
                case BlackjackAction.Hit:
                    Hit();
                    break;
                case BlackjackAction.Stand:
                    Stand();
                    break;
                case BlackjackAction.Double:
                    if (CanDoubleDown())
                    {
                        DoubleDown();
                    }
                    else
                    {
                        Hit();
                    }

                    break;
                case BlackjackAction.Split:
                    if (CanSplit())
                    {
                        Split();
                    }
                    else
                    {
                        Hit();
                    }

                    break;
                case BlackjackAction.Surrender:
                    if (CanSurrender())
                    {
                        Surrender();
                    }
                    else
                    {
                        Hit();
                    }

                    break;
            }
        }
    }
}
