using System.Text;
using Peat.Cli.Commands.Models;
using Peat.Core.Optimization.Common;

namespace Peat.Cli;

public class ConsoleTransformationReportGenerator(bool useEmojis = true) : ITransformationReportGenerator
{
    private bool UseEmojis { get; } = useEmojis;

    private static class ConsoleFormatting
    {
        public const string Reset = "\e[0m";
        public const string Cyan = "\e[36m";
        public const string White = "\e[37m";
        public const string Gray = "\e[90m";
        public const string Yellow = "\e[33m";
        public const string Red = "\e[31m";
        public const string Green = "\e[32m";
        public const string WhiteOnBlue = "\e[44m\e[37m";
    }

    private static class ReportSymbols
    {
        public static string GetStepSymbol(bool useEmojis) => useEmojis ? "🔄 " : "-> ";
        public static string GetTimeSymbol(bool useEmojis) => useEmojis ? "⏱️ " : "[TIME] ";
        public static string GetTypeSymbol(bool useEmojis) => useEmojis ? "🏷️ " : "[TYPE] ";
        public static string GetDescriptionSymbol(bool useEmojis) => useEmojis ? "📝 " : "[DESC] ";
        public static string GetPositionSymbol(bool useEmojis) => useEmojis ? "📍 " : "[POS] ";
        public static string GetOriginalSymbol(bool useEmojis) => useEmojis ? "⚪ " : "[IN] ";
        public static string GetResultSymbol(bool useEmojis) => useEmojis ? "🟢 " : "[OUT] ";
    }

    public string GenerateReport(OptimizationStepViewModel context)
    {
        var report = new StringBuilder();
        
        AppendHeader(report, context);
        
        foreach (var step in context.TransformationSteps!)
        {
            AppendStep(report, step);
        }

        return report.ToString();
    }

    public string GeneratePlainReport(TransformationContext context)
    {
        var report = new StringBuilder();
        
        AppendPlainHeader(report, context);
        
        foreach (var step in context.Steps)
        {
            AppendPlainStep(report, step);
        }

        return report.ToString();
    }

    private void AppendHeader(StringBuilder sb, OptimizationStepViewModel context)
    {
        AppendFormattedLine(sb, ConsoleFormatting.Cyan,
            $"Transformation Report for {context.Name}");
        AppendFormattedLine(sb, ConsoleFormatting.Cyan,
            $"Total Steps: {context.TransformationSteps!.Count}");
        sb.AppendLine();
    }

    private void AppendPlainHeader(StringBuilder sb, TransformationContext context)
    {
        sb.AppendLine($"Transformation Report for {context.OptimizerId}");
        sb.AppendLine($"Total Steps: {context.TotalTransformations}");
        sb.AppendLine();
    }

    private void AppendStep(StringBuilder sb, TransformationStepViewModel step)
    {
        AppendStepHeader(sb, step);
        AppendStepDetails(sb, step);
        AppendTransformationResults(sb, step);
        sb.AppendLine();
    }

    private void AppendPlainStep(StringBuilder sb, TransformationStep step)
    {
        sb.AppendLine($"Step ID: {step.Id}");
        sb.AppendLine($"Time: {step.Timestamp:yyyy-MM-dd HH:mm:ss.fff}");
        sb.AppendLine($"Type: {step.Operation.Type}");
        sb.AppendLine($"Description: {step.Operation.Description}");
        sb.AppendLine($"Position: {step.Position.StartIndex}-{step.Position.EndIndex}");
        sb.AppendLine($"Original: {step.Position.OriginalFragment}");
        sb.AppendLine($"Result: {string.Join(" ", step.Operation.ResultTokens.Select(t => t.Value))}");
        sb.AppendLine();
    }

    private void AppendStepHeader(StringBuilder sb, TransformationStepViewModel step)
    {
        AppendFormattedLine(sb, ConsoleFormatting.WhiteOnBlue,
            $"{ReportSymbols.GetStepSymbol(UseEmojis)}Step ID: {step.Id}");
    }

    private void AppendStepDetails(StringBuilder sb, TransformationStepViewModel step)
    {
        AppendFormattedLine(sb, ConsoleFormatting.Gray,
            $"{ReportSymbols.GetTimeSymbol(UseEmojis)}Time: {step.Timestamp:yyyy-MM-dd HH:mm:ss.fff}");
        
        AppendFormattedLine(sb, ConsoleFormatting.Yellow,
            $"{ReportSymbols.GetTypeSymbol(UseEmojis)}Type: {step.Operation.Type}");
        
        AppendFormattedLine(sb, ConsoleFormatting.White,
            $"{ReportSymbols.GetDescriptionSymbol(UseEmojis)}Description: {step.Operation.Description}");
        
        AppendFormattedLine(sb, ConsoleFormatting.Gray,
            $"{ReportSymbols.GetPositionSymbol(UseEmojis)}Position: {step.Position.StartIndex}-{step.Position.EndIndex}");
    }

    private void AppendTransformationResults(StringBuilder sb, TransformationStepViewModel step)
    {
        AppendFormattedLine(sb, ConsoleFormatting.Red,
            $"{ReportSymbols.GetOriginalSymbol(UseEmojis)}Original: {step.Position.OriginalFragment}");
        
        AppendFormattedLine(sb, ConsoleFormatting.Green,
            $"{ReportSymbols.GetResultSymbol(UseEmojis)}Result: {string.Join(" ", step.Operation.ResultTokens.Select(t => t.Value))}");
    }

    private static void AppendFormattedLine(StringBuilder sb, string color, string content)
        => sb.AppendLine($"{color}{content}{ConsoleFormatting.Reset}");
}