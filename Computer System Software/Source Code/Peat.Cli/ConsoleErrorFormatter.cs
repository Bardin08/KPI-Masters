using Peat.Application.Expressions.Models;

namespace Peat.Cli;

public static class ConsoleErrorFormatter
{
    public static void PrintError(string expression, ExpressionError error)
    {
        var lines = new List<string>
        {
            "❌ Error:",
            expression,
            new string(' ', error.Position) + "^ " + error.Message
        };

        Console.WriteLine(string.Join('\n', lines));
    }

    public static void PrintErrors(string expression, IEnumerable<ExpressionError> errors)
    {
        foreach (var error in errors.OrderBy(e => e.Position))
        {
            Console.WriteLine();
            PrintError(expression, error);
        }
    }
}