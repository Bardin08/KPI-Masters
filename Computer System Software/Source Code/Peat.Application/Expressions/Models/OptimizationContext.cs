namespace Peat.Application.Expressions.Models;

public record OptimizationContext
{
    public required IReadOnlyList<OptimizationStep> Steps { get; init; }
    public required TimeSpan OptimizationTime { get; init; }
}