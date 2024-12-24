namespace Peat.Core.Optimization.Common;

public class OptimizerException(string message, Exception? innerException = null)
    : Exception(message, innerException);
    
public class UnaryOperatorException(string message, int position) : Exception(message)
{
    public int Position { get; } = position;
}
