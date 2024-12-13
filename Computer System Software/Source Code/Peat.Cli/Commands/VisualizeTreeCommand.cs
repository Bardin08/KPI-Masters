using Peat.Application.Expressions.Services;
using Peat.Application.Visualization;
using Peat.Core.Lexer;
using Peat.Core.Syntax.Nodes;
using Peat.Core.Syntax.Parser;

namespace Peat.Cli.Commands;

public record VisualizeTreeCommand(string[] Args, Dictionary<string, string> Options)
{
    private readonly IExpressionService _expressionService = new ExpressionService(
        new Lexer(), new PrattParser());

    public void Handle()
    {
        if (Args.Length < 2)
        {
            Console.WriteLine("Error: Expression required for tree visualization");
            return;
        }

        var ast = _expressionService.GetAst(Args[1]);

        var outputFormat = DetermineOutputFormat();
        var output = GenerateTreeVisualization(ast, outputFormat);

        HandleOutput(output, outputFormat);
    }

    private string DetermineOutputFormat()
    {
        string[] outputAcceptable = ["json", "mermaid", "text"];

        var format = Options.TryGetValue("output", out var longOutput) ? longOutput
            : Options.GetValueOrDefault("o", "text");

        return outputAcceptable.Contains(format.ToLower())
            ? format.ToLower()
            : "text";
    }

    private static string GenerateTreeVisualization(INode ast, string format)
    {
        return format switch
        {
            "json" => new JsonTreeVisualizer().Visualize(ast),
            "mermaid" => new MermaidTreeVisualizer().Visualize(ast),
            "text" => new ColoredPlainTextTreeVisualizer().Visualize(ast),
            _ => new PlainTextVisualizer().Visualize(ast)
        };
    }

    private void HandleOutput(string output, string format)
    {
        Console.WriteLine(output);

        var shortSavePath = string.Empty;
        // Check if save option is provided (both short and long formats)
        var hasSaveOption = Options.TryGetValue("save", out var longSavePath) ||
                            Options.TryGetValue("s", out shortSavePath) &&
                            !string.IsNullOrEmpty(longSavePath) || !string.IsNullOrEmpty(shortSavePath);

        if (!hasSaveOption)
            return;

        var savePath = longSavePath ?? shortSavePath;

        if (hasSaveOption && !string.IsNullOrWhiteSpace(savePath))
        {
            try
            {
                var fileExtension = format switch
                {
                    "json" => ".json",
                    "mermaid" => ".mmd",
                    "text" => ".txt",
                    _ => ".txt"
                };

                var path = Path.Combine(savePath, $"ast-{output.GetHashCode()}{fileExtension}");
                File.WriteAllText(path, output);
                Console.WriteLine($"Tree visualization saved to: {path}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving file: {ex.Message}");
            }
        }
        else
        {
            // Print to console if no save option
            Console.WriteLine(output);
        }
    }
}