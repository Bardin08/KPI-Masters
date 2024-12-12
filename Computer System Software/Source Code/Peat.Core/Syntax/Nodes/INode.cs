using Peat.Core.Lexer;

namespace Peat.Core.Syntax.Nodes;

public interface INode
{
    TokenType Type { get; }
    string Value { get; }
}