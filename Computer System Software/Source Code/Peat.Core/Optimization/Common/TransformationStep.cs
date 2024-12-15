namespace Peat.Core.Optimization.Common;

public record TransformationStep(
    Guid Id,
    DateTime Timestamp,
    TransformOperation Operation,
    TransformPosition Position,
    string OptimizerId
);