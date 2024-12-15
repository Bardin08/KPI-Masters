using Peat.Core.Lexer;
using Peat.Core.Optimization.Common;

namespace Peat.Core.Optimization;

public abstract class BaseOptimizer : ITokenOptimizer
{
    public TransformationContext TransformationContext = new(Guid.NewGuid().ToString("N")[7..12]);

    public virtual IEnumerable<Token> Optimize(IEnumerable<Token> tokens)
    {
        return tokens;
    }

    protected static bool IsNumber(Token token, string value)
        => token.TokenType == TokenType.Number && token.Value == value;

    protected static bool IsPrevToken(List<Token> tokenList, int position, TokenType expected)
    {
        if (position is 0) return false;
        return tokenList[position - 1].TokenType == expected;
    }
}