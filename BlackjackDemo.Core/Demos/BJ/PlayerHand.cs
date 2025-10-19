namespace Demo.Demos.BJ;

public class PlayerHand(IEnumerable<Card> initialCards, decimal bet, HandOutcome? outcome = null) : Hand(initialCards)
{
    public decimal Bet { get; private set; } = bet;
    public HandOutcome? Outcome { get; private set; } = outcome;

    public void DoubleBet() => Bet *= 2;

    public PlayerHand Split()
    {
        if (!IsPair) throw new InvalidOperationException("Cannot split a hand that is not a pair.");

        var cardToRemove = _cards[1];
        _cards.RemoveAt(1);
        CalculateHandValue();

        return new PlayerHand(new List<Card> { cardToRemove }, Bet);
    }

    public void SetOutcome(HandOutcome outcome) => Outcome = outcome;
}