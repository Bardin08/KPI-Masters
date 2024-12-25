using Peat.Core.Lexer;
using Peat.Core.Optimization.Common;

namespace Peat.Core.Optimization;

public abstract class BaseOptimizer : ITokenOptimizer
{
    public TransformationContext TransformationContext { get; private set; } =
        new(Guid.NewGuid().ToString("N")[7..12]);

    public virtual IEnumerable<Token> Optimize(IEnumerable<Token> tokens)
    {
        return tokens;
    }
}