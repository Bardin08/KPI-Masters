namespace Peat.Application.Expressions.Models;

public record ParallelizationContext
{
    public required int ParallelizationLevel { get; init; }
    public required int EstimatedProcessorCount { get; init; }
    public required TimeSpan ParallelizationTime { get; init; }
}