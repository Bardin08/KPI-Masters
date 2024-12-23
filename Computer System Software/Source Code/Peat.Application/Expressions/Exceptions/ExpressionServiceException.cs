using Peat.Application.Expressions.Models;

namespace Peat.Application.Expressions.Exceptions;

public class ExpressionServiceException(ExpressionError error) : Exception
{
    public ExpressionError Error { get; } = error;
}