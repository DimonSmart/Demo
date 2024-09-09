namespace Demo.Demos.BJ;

public interface IHandValues
{
    bool AllCardFacedUp { get; }
    int HandValue { get; }
    bool IsBusted { get; }
    bool IsSoft { get; }
    bool IsPair { get; }
}