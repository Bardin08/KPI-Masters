using Peat.Core.Lexer;
using Peat.Core.Syntax.Nodes;

namespace Peat.Core.Optimization.Common;

public class TransformationContext(string optimizerId)
{
    private readonly List<TransformationStep> _steps = new();

    public string OptimizerId { get; } = optimizerId;

    public IReadOnlyList<TransformationStep> Steps => _steps.AsReadOnly();
    public int TotalTransformations => _steps.Count;

    public void RecordTransformation(
        string type,
        string description,
        IReadOnlyList<Token> originalTokens,
        IReadOnlyList<Token> resultTokens,
        INode? originalNode,
        INode? resultNode)
    {
        var position = DetermineTransformPosition(originalTokens);
        var operation = new TransformOperation(
            type,
            description,
            originalTokens,
            resultTokens,
            originalNode,
            resultNode);

        var step = new TransformationStep(
            Guid.NewGuid(),
            DateTime.UtcNow,
            operation,
            position,
            OptimizerId);

        _steps.Add(step);
    }

    private TransformPosition DetermineTransformPosition(IReadOnlyList<Token> tokens)
    {
        ArgumentNullException.ThrowIfNull(tokens);
        if (!tokens.Any())
            return new TransformPosition(0, 0, string.Empty);

        var start = tokens.Min(t => t.Position);
        var end = tokens.Max(t => t.Position);
        var fragment = string.Join(" ", tokens.Select(t => t.Value));

        return new TransformPosition(start, end, fragment);
    }
}