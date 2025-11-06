namespace Demo.Demos.BJ
{
    public class BlackjackGameWithPlayer : BlackjackGameBase
    {
        public BlackjackGameWithPlayer(
            Shoe shoe,
            ILogger? logger = null,
            BlackjackRulesOptions? rulesOptions = null)
            : base(shoe, logger, rulesOptions)
        {
        }
    }
}
