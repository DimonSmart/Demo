namespace Demo.Demos.BJ
{
    public class AutoPlayerGame : BlackjackGameBase
    {
        private readonly StrategyTable _strategyTable;

        public AutoPlayerGame(Shoe shoe, StrategyTable strategyTable, ILogger? logger = null) : base(shoe, logger)
        {
            _strategyTable = strategyTable;
        }

        protected override void PlayPlayerHands()
        {
            foreach (var hand in _playerHands.ToArray())
            {
                PlayHandAccordingToStrategy(hand);
            }
        }

        private void PlayHandAccordingToStrategy(Hand hand)
        {
            // AutoPlayer logic using _strategyTable
        }
    }
}
