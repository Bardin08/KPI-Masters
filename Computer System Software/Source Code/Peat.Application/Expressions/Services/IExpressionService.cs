using Peat.Application.Expressions.Models;

namespace Peat.Application.Expressions.Services;

public interface IExpressionService
{
    ValidationResult Validate(string expression);
    ParallelTreeResponse GetParallelTree(string expression);
}