using Peat.Core.Lexer;

namespace Peat.Core.Syntax.Nodes;

public record BinaryNode(INode Left, string Operator, INode Right) : INode
{
    public TokenType Type => TokenType.Operator;
    public string Value => Operator;
}