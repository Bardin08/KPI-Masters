using Peat.Core.Lexer;

namespace Peat.Core.Syntax.Nodes;

public record VariableNode(string Name) : INode 
{
    public TokenType Type => TokenType.Variable;
    public string Value => Name;
}