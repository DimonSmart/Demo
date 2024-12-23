using Demo.Demos.BJ;
using GeneticAlgorithm.GeneralGenetic;
using System.CommandLine;

namespace BJTableCalculator;
internal class Program
{
    static async Task<int> Main(string[] args)
    {
        var calculateCommand = new Command("calculate-strategy", "Perform a strategy calculation")
        {
            new Argument<string>("rules.json", "The name of the rules file")
        };

        calculateCommand.SetHandler(async (string rulesFile) =>
        {
            Console.WriteLine($"Command: calculate-strategy | Rules file: {rulesFile}");
            var shoe = new Shoe(DeckFactory.CreateShuffledDecks(6, RandomProvider.Shared).Cards, redCardPosition: 100);
            var game = new BlackjackGameAuto(shoe, GetStrategyTable(), logger: ConsoleLogger.Shared);
            await game.PlayGameAsync();

        }, new Argument<string>("rules.json"));


        var continueCommand = new Command("continue-strategy-calculation", "Continue an existing strategy calculation")
        {

            new Argument<string>("rules.json", "The name of the rules file")
        };


        continueCommand.SetHandler((string rulesFile) =>
        {
            Console.WriteLine($"Command: continue-strategy-calculation | Rules file: {rulesFile}");

        }, new Argument<string>("rules.json"));


        var rootCommand = new RootCommand("BJ game genetic algorithm base strategy calculator");
        rootCommand.AddCommand(calculateCommand);
        rootCommand.AddCommand(continueCommand);

        return await rootCommand.InvokeAsync(args);
    }

    private static StrategyTable GetStrategyTable() =>
           new(Enumerable.Range(0, 22 * 11 + 22 * 11 + 14 * 11)
               .Select(_ => (BlackjackAction)RandomProvider.Shared.Next(5))
               .ToArray());
}

