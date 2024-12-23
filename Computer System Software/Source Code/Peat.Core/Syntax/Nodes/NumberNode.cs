using Peat.Core.Lexer;

namespace Peat.Core.Syntax.Nodes;

public record NumberNode(string Value) : INode
{
    public TokenType Type => TokenType.Number;
}