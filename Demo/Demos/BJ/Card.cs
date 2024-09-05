namespace Demo.Demos.BJ
{
    public readonly record struct Card(Suit Suit, Rank Rank)
    {
        public int Value => (int)Rank;
    }
}
