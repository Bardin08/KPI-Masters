using Peat.Core.Lexer;

namespace Peat.Core.Syntax.Nodes;

public record FunctionNode(string Name, IReadOnlyList<INode> Arguments) : INode 
{
    public TokenType Type => TokenType.Function;
    public string Value => Name;
}