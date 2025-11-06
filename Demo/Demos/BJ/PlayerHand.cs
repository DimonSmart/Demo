using System.Collections.Generic;

namespace Demo.Demos.BJ;

public class PlayerHand : Hand
{
    public PlayerHand(IEnumerable<Card> initialCards, decimal bet, HandOutcome? outcome = null, bool wasSplitAce = false)
        : base(initialCards)
    {
        Bet = bet;
        Outcome = outcome;
        WasSplitAce = wasSplitAce;
    }

    public decimal Bet { get; private set; }
    public HandOutcome? Outcome { get; private set; }
    public bool WasSplitAce { get; private set; }
    public bool IsCompleted { get; private set; }

    public void DoubleBet() => Bet *= 2;

    public PlayerHand Split()
    {
        if (!IsPair) throw new InvalidOperationException("Cannot split a hand that is not a pair.");

        var cardToRemove = _cards[1];
        _cards.RemoveAt(1);
        CalculateHandValue();

        var splitFromAces = _cards[0].Rank == Rank.Ace && cardToRemove.Rank == Rank.Ace;
        if (splitFromAces)
        {
            WasSplitAce = true;
        }

        return new PlayerHand(new List<Card> { cardToRemove }, Bet, wasSplitAce: splitFromAces);
    }

    public void MarkCompleted() => IsCompleted = true;

    public void SetOutcome(HandOutcome outcome) => Outcome = outcome;
}
