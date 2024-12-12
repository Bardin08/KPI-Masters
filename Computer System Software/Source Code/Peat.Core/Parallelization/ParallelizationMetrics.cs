namespace Peat.Core.Parallelization;

public record ParallelizationMetrics
{
    public required int Height { get; init; }
    public required int MaxWidth { get; init; }
    public required int TotalNodes { get; init; }
    public required IReadOnlyDictionary<int, int> LevelDistribution { get; init; }
}