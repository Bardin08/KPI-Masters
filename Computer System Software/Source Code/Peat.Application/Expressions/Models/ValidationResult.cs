namespace Peat.Application.Expressions.Models;

public record ValidationResult
{
    public bool IsValid { get; init; }
    public IReadOnlyList<ExpressionError> Errors { get; init; } = [];
    public TimeSpan ValidationTime { get; init; }
}