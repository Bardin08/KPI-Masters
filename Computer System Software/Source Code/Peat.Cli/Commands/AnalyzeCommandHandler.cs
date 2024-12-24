using Peat.Application.Expressions.Services;
using Peat.Core.Lexer;
using Peat.Core.Syntax.Parser;

namespace Peat.Cli.Commands;

internal record AnalyzeExpressionCommandHandler(string[] Args, Dictionary<string, string> Options) : ICliCommandHandler
{
    private readonly IExpressionService _expressionService = new ExpressionService(
        new Lexer(), new PrattParser());

    public void Handle()
    {
        if (Args.Length < 2)
        {
            Console.WriteLine("Error: Expression required for analysis");
            return;
        }

        var expression = Args[1];

        Console.WriteLine($"\n🔍 Analyzing expression: \"{expression}\"");
        Console.WriteLine("═════════════════════════════════════════════════\n");

        try
        {
            var result = _expressionService.Validate(expression);

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
}