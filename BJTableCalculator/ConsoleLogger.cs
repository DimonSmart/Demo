using Demo.Demos.BJ;

namespace BJTableCalculator;

public class ConsoleLogger : ILogger
{
    public static readonly ConsoleLogger Shared = new ConsoleLogger();

    private ConsoleLogger() { }

    public void Info(string message)
    {
        Console.WriteLine(message);
    }
}
