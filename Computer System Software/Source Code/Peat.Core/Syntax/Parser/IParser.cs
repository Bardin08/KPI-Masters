using Peat.Core.Lexer;
using Peat.Core.Syntax.Nodes;

namespace Peat.Core.Syntax.Parser;

public interface IParser
{
    INode Parse(IEnumerable<Token> tokens);
}