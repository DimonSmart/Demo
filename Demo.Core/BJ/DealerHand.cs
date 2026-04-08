namespace Demo.Demos.BJ;

public class DealerHand(IEnumerable<Card> initialCards) : Hand(initialCards)
{
    public void FlipSecondCard() => _cards[1] = _cards[1] with { IsFaceUp = true };

    public void FlipCard(Card card)
    {
        var index = _cards.IndexOf(card);
        _cards[index] = _cards[index] with { IsFaceUp = true };
    }
}