namespace Peat.Application.Expressions.Models;

public record TransformationStep(
    Guid Id,
    DateTime Timestamp,
    TransformOperation Operation,
    TransformPosition Position
);