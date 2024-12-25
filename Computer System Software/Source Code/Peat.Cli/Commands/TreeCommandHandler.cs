using System.Text;
using System.Text.Json;
using Peat.Application.Expressions.Models;
using Peat.Application.Expressions.Services;
using Peat.Application.Visualization;
using Peat.Cli.Commands.Models;
using Peat.Core.Lexer;
using Peat.Core.Optimization.Common;
using Peat.Core.Syntax.Nodes;
using Peat.Core.Syntax.Parser;

namespace Peat.Cli.Commands;

public record TreeCommandHandler(string[] Args, Dictionary<string, string> Options) : ICliCommandHandler
{
    private static readonly string[] SupportedExportFormats = ["json", "mermaid", "text"];

    private readonly IExpressionService _expressionService = new ExpressionService(
        new Lexer(), new PrattParser());

    private readonly ITransformationReportGenerator _reportGenerator = new ConsoleTransformationReportGenerator();

    public void Handle()
    {
        if (Args.Length < 2)
        {
            Console.WriteLine("Error: Expression required for tree visualization");
            return;
        }

        HandleInternal();
    }

    private void HandleInternal()
    {
        var response = _expressionService.GetParallelTree(Args[1]);
        var outputFormat = DetermineOutputFormat();

        if (!response.Context.Validation.IsValid)
        {
            ConsoleErrorFormatter.PrintErrors(Args[1], response.Context.Validation.Errors);
            return;
        }

        var output = GetOutputString(response, outputFormat);
        SaveOutput(output, outputFormat);
    }

    private string GetOutputString(ParallelTreeResponse response, string outputFormat)
    {
        var responseViewModel = new VisualizationData(
            GenerateTreeVisualization(response.Tree, outputFormat),
            new ProcessingResult(response.Context),
            response.ProcessingTime.TotalMilliseconds);

        var output = outputFormat switch
        {
            "json" => JsonSerializer.Serialize(responseViewModel, new JsonSerializerOptions { WriteIndented = true }),
            _ => FormatOutputString(responseViewModel, Options.ContainsKey("verbose") || Options.ContainsKey("v"))
        };
        return output;
    }

    private string DetermineOutputFormat() =>
        Options.TryGetValue("output", out var longOutput)
            ? longOutput
            : Options.GetValueOrDefault("o", "text").ToLower() switch
            {
                var f when SupportedExportFormats.Contains(f) => f,
                _ => "text"
            };

    private static string GenerateTreeVisualization(INode ast, string format) =>
        format switch
        {
            "json" => new JsonTreeVisualizer().Visualize(ast),
            "mermaid" => new MermaidTreeVisualizer().Visualize(ast),
            "text" => new ColoredPlainTextTreeVisualizer().Visualize(ast),
            _ => new PlainTextVisualizer().Visualize(ast)
        };

    private string FormatOutputString(VisualizationData data, bool verbose)
    {
        var sb = new StringBuilder();
        sb.AppendLine("🌳 Tree Visualization:");
        sb.AppendLine(data.Tree);
        sb.AppendLine("\n📊 Performance Metrics:");
        sb.AppendLine($"⚡ Parallelization Level: {data.Metrics.Parallelization.Level}");
        sb.AppendLine($"🖥️ Estimated Processors: {data.Metrics.Parallelization.EstimatedProcessors}");
        sb.AppendLine($"⏱️ Processing Time: {data.ProcessingTime:F2}ms");

        if (data.Metrics.Optimization.Steps.Count > 0)
        {
            sb.AppendLine("\n🔄 Optimization Steps:");
            foreach (var step in data.Metrics.Optimization.Steps)
            {
                sb.AppendLine($"  ✨ {step.Name}: {step.TokensBefore} → {step.TokensAfter} tokens");

                if (verbose && !step.TransformationSteps?.Any() is true)
                {
                    sb.AppendLine($"  {_reportGenerator.GenerateReport(step)}");
                }
            }
        }

        return sb.ToString();
    }

    private void SaveOutput(string output, string format)
    {
        Console.WriteLine(output);

        var shortSavePath = string.Empty;
        if (!Options.TryGetValue("save", out var longSavePath) &&
            !Options.TryGetValue("s", out shortSavePath))
            return;

        var savePath = longSavePath ?? shortSavePath;
        if (string.IsNullOrWhiteSpace(savePath))
            return;

        try
        {
            var fileExtension = format switch
            {
                "json" => ".json",
                "mermaid" => ".mmd",
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
}