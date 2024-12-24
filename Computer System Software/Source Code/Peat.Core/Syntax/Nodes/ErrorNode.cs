using Peat.Core.Lexer;

namespace Peat.Core.Syntax.Nodes;

public class ErrorNode(string message) : INode
{
    public TokenType Type => TokenType.Variable;
    public string Value { get; } = message;
}