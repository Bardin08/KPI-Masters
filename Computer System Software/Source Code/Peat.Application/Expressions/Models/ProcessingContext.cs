namespace Peat.Application.Expressions.Models;

public record ProcessingContext
{
    public required ValidationResult Validation { get; init; }
    public required OptimizationContext Optimization { get; init; }
    public required ParallelizationContext Parallelization { get; init; }
}