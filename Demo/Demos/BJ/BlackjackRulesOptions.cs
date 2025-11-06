namespace Demo.Demos.BJ;

public class BlackjackRulesOptions
{
    public static BlackjackRulesOptions Default { get; } = new();

    public bool CanHitAfterSplitAces { get; init; } = false;
}
