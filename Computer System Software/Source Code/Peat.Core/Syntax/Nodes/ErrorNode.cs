using Peat.Core.Lexer;

namespace Peat.Core.Syntax.Nodes;

internal class ErrorNode(string message) : INode
{
    public TokenType Type => TokenType.Variable;
    public string Value { get; } = message;
}