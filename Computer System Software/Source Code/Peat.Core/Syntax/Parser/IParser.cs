using Peat.Core.Lexer;

namespace Peat.Core.Syntax.Nodes;

public interface IParser
{
    INode Parse(IEnumerable<Token> tokens);
}