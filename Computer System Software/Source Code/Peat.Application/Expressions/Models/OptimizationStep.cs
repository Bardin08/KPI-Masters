using Peat.Core.Lexer;

namespace Peat.Application.Expressions.Models;

public record OptimizationStep
{
    public required string OptimizerName { get; init; }
    public required IReadOnlyList<Token> InputTokens { get; init; }
    public required IReadOnlyList<Token> OutputTokens { get; init; }
    public List<TransformationStep>? Transformations { get; set; }
}