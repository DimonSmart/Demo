namespace Demo.Demos.BJ
{
    [Flags]
    public enum Rank
    {
        Two = 2,
        Three = 3,
        Four = 4,
        Five = 5,
        Six = 6,
        Seven = 7,
        Eight = 8,
        Nine = 9,
        Ten = 10,
        Jack = 10 | (1 << 4),
        Queen = 10 | (1 << 5),
        King = 10 | (1 << 6),
        Ace = 11
    }
}
