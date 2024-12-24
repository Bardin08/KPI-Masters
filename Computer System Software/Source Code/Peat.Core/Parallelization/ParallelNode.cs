using Peat.Core.Lexer;
using Peat.Core.Syntax.Nodes;

namespace Peat.Core.Parallelization;

public record ParallelNode : INode
{
    public required INode OriginalNode { get; init; }
    public int Level { get; set; } // settable now, assigned after build
    public required int Order { get; init; }
    public IReadOnlyList<ParallelNode> Dependencies { get; init; } = [];

    public TokenType Type => OriginalNode.Type;
    public string Value => OriginalNode.Value;
}