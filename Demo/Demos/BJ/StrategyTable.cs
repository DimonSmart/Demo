namespace Demo.Demos.BJ
{
    public class StrategyTable
    {
        // 22 possible player totals (2-21) vs 11 dealer cards (2-Ace)
        private const int HardTotalSize = 22 * 11;
        private const int SoftTotalSize = 22 * 11;

        // 14 pairs (2,2 through A,A) vs 11 dealer cards
        private const int PairSplittingSize = 14 * 11;

        private readonly BlackjackAction[] _strategy;

        public StrategyTable(BlackjackAction[] strategy)
        {
            if (strategy.Length != HardTotalSize + SoftTotalSize + PairSplittingSize)
                throw new ArgumentException("Invalid strategy array length.");

            _strategy = strategy.ToArray();
        }

        public StrategyTable(StrategyTable strategyTable) : this(strategyTable._strategy)
        {
        }

        public BlackjackAction GetAction(IHandValues hand, int dealerCardValue)
        {
            return hand.IsPair
                ? GetPairSplittingAction(hand.HandValue / 2, dealerCardValue)
                : GetTotalAction(hand.HandValue, dealerCardValue, hand.IsSoft);
        }

        private BlackjackAction GetTotalAction(int playerTotal, int dealerCardValue, bool isSoft)
        {
            int baseOffset = isSoft ? HardTotalSize : 0;
            var index = baseOffset + (playerTotal - 2) * 11 + (dealerCardValue - 2);
            return _strategy[index];
        }

        private BlackjackAction GetPairSplittingAction(int pairRank, int dealerCardValue)
        {
            var index = HardTotalSize + SoftTotalSize + (pairRank - 2) * 11 + (dealerCardValue - 2);
            return _strategy[index];
        }
    }
}
