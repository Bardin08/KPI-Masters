using System.Text;
using Peat.Cli.Commands.Models;

namespace Peat.Cli.Formatters;

public class DefaultOutputFormatter(ITransformationReportGenerator reportGenerator) : IOutputFormatter
{
    public string Format(VisualizationData data, bool verbose)
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

                if (verbose && step.TransformationSteps?.Any() is true)
                {
                    sb.AppendLine($"  {reportGenerator.GenerateReport(step)}");
                }
            }
        }

        return sb.ToString();
    }
}