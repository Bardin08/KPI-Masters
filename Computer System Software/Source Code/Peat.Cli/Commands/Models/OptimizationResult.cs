namespace Peat.Cli.Commands.Models;

public record OptimizationResult(int StepsCount, double TimeMs, IReadOnlyList<OptimizationStepViewModel> Steps);