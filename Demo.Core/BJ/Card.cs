namespace Demo.Demos.BJ
{
    public readonly record struct Card(Suit Suit, Rank Rank, bool IsFaceUp = true)
    {
        public int Value => (int)Rank & 0xF;
    }
}
