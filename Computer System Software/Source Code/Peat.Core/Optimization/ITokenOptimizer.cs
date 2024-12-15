using Peat.Core.Lexer;

namespace Peat.Core.Optimization;

public interface ITokenOptimizer
{
    IEnumerable<Token> Optimize(IEnumerable<Token> tokens);
}