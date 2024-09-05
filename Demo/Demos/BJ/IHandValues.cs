namespace Demo.Demos.BJ
{
    public interface IHandValues
    {
        int HandValue { get; }
        bool IsBusted { get; }
        bool IsSoft { get; }
        bool IsPair { get; }
    }

}
