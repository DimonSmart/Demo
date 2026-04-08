using System;
using System.Linq;

namespace Demo.Demos.BJ
{
    public class StrategyTable
    {
        public const int DealerCardMinValue = 2;
        public const int DealerCardMaxValue = 11;
        public const int DealerColumnCount = DealerCardMaxValue - DealerCardMinValue + 1;

        public const int MinPlayerTotal = 2;
        public const int MaxPlayerTotal = 21;
        private const int PlayerTotalRange = MaxPlayerTotal - MinPlayerTotal + 1;

        public const int PairRankCount = 13;

        public const int HardSectionSize = PlayerTotalRange * DealerColumnCount;
        public const int SoftSectionSize = PlayerTotalRange * DealerColumnCount;
        public const int PairSectionSize = PairRankCount * DealerColumnCount;
        public const int StrategySize = HardSectionSize + SoftSectionSize + PairSectionSize;

        private readonly BlackjackAction[] _strategy;

        public StrategyTable(BlackjackAction[] strategy)
        {
            if (strategy.Length != StrategySize)
            {
                throw new ArgumentException("Invalid strategy array length.", nameof(strategy));
            }

            _strategy = strategy.ToArray();
        }

        public StrategyTable(StrategyTable strategyTable) : this(strategyTable._strategy)
        {
        }

        public BlackjackAction GetAction(IHandValues hand, int dealerCardValue)
        {
            if (hand is null)
            {
                throw new ArgumentNullException(nameof(hand));
            }

            ValidateDealerCardValue(dealerCardValue);

            return hand.IsPair
                ? GetPairSplittingAction(hand.PairRank, dealerCardValue)
                : GetTotalAction(hand.HandValue, dealerCardValue, hand.IsSoft);
        }

        private static void ValidateDealerCardValue(int dealerCardValue)
        {
            if (dealerCardValue < DealerCardMinValue || dealerCardValue > DealerCardMaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(dealerCardValue), dealerCardValue,
                    $"Dealer card value must be between {DealerCardMinValue} and {DealerCardMaxValue}.");
            }
        }

        private BlackjackAction GetTotalAction(int playerTotal, int dealerCardValue, bool isSoft)
        {
            if (playerTotal < MinPlayerTotal || playerTotal > MaxPlayerTotal)
            {
                throw new ArgumentOutOfRangeException(nameof(playerTotal), playerTotal,
                    $"Player total must be between {MinPlayerTotal} and {MaxPlayerTotal}.");
            }

            var baseOffset = isSoft ? HardSectionSize : 0;
            var index = baseOffset + (playerTotal - MinPlayerTotal) * DealerColumnCount +
                        (dealerCardValue - DealerCardMinValue);
            return _strategy[index];
        }

        private BlackjackAction GetPairSplittingAction(int pairRankIndex, int dealerCardValue)
        {
            if (pairRankIndex < 0 || pairRankIndex >= PairRankCount)
            {
                throw new ArgumentOutOfRangeException(nameof(pairRankIndex), pairRankIndex,
                    $"Pair rank index must be between 0 and {PairRankCount - 1}.");
            }

            var index = HardSectionSize + SoftSectionSize +
                        pairRankIndex * DealerColumnCount + (dealerCardValue - DealerCardMinValue);
            return _strategy[index];
        }
    }
}
