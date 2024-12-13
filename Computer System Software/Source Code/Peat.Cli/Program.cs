using Peat.Application.Expressions.Services;
using Peat.Cli.Commands;
using Peat.Core.Lexer;
using Peat.Core.Syntax.Parser;

namespace Peat.Cli;

public static class Program
{
    private static readonly IExpressionService ExpressionService = new ExpressionService(new Lexer(), new Parser());

    public static async Task Main(string[] args)
    {
        if (args.Length == 0)
        {
            ShowHelp();
            return;
        }

        var command = args[0].ToLower();
        var options = ParseOptions(args);

        switch (command)
        {
            case "analyze":
                if (args.Length < 2)
                {
                    Console.WriteLine("Error: Expression required for analysis");
                    return;
                }

                await AnalyzeExpression(args[1]);
                break;
            case "tree":
                if (args.Length < 2)
                {
                    Console.WriteLine("Error: Expression required for analysis");
                    return;
                }

                new VisualizeTreeCommand(args, options).Handle();
                break;
        }
    }

    private static async Task AnalyzeExpression(string expression)
    {
        Console.WriteLine($"\n🔍 Analyzing expression: \"{expression}\"");
        Console.WriteLine("═════════════════════════════════════════════════\n");

        try
        {
            var result = await ExpressionService.ValidateAsync(expression);

            if (result.IsValid)
            {
                Console.WriteLine($"✅ Result: {(result.IsValid ? "Valid" : "Invalid")}");
                Console.WriteLine($"⏱️ Time: {result.ValidationTime.TotalMilliseconds:F2}ms\n");
            }
            else
            {
                Console.WriteLine("❌ Analysis failed:");
                ConsoleErrorFormatter.PrintErrors(expression, result.Errors);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"💥 Error: {ex.Message}");
        }
    }

    private static Dictionary<string, string> ParseOptions(string[] args)
    {
        var options = new Dictionary<string, string>();
        for (var i = 0; i < args.Length; i++)
        {
            if (!args[i].StartsWith('-'))
                continue;

            var key = args[i].TrimStart('-');
            var value = i + 1 < args.Length ? args[i + 1] : null;
            options[key] = value ?? "true";
            i++;
        }

        return options;
    }

    private static void ShowHelp()
    {
        Console.WriteLine(
            """

            PEAT - Parallel Expression Analysis Tool

            Commands:
              analyze <expression>          Run full analysis pipeline

            Examples:
              peat analyze "a + b * (c - d)"

            """);
    }
}