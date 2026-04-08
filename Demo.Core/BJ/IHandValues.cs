namespace Demo.Demos.BJ;

public interface IHandValues
{
    bool AllCardFacedUp { get; }
    int HandValue { get; }
    bool IsBusted { get; }
    bool IsSoft { get; }
    bool IsPair { get; }
    int PairRank { get; }
}


public record HandOutcome(decimal Outcome, string OutcomeDescription);

