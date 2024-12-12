namespace Peat.Application.Expressions.Models;

public record ExpressionError
{
    public required string Message { get; init; }
    public ErrorType Type { get; init; }
    public int Position { get; init; }
}