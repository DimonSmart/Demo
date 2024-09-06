namespace Demo.Demos.BJ
{
    public class ManualPlayerGame : BlackjackGameBase
    {
        public ManualPlayerGame(Shoe shoe, ILogger? logger = null) : base(shoe, logger)
        {
        }

        protected override void PlayPlayerHands()
        {
            foreach (var hand in _playerHands.ToArray())
            {
                // Wait for user input to make decisions for this hand (via UI or other input method)
            }
        }
    }
}
