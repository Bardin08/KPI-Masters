using Peat.Application.Expressions.Models;
using Peat.Core.Syntax.Nodes;

namespace Peat.Application.Expressions.Services;

public interface IExpressionService
{
    ValidationResult Validate(string expression);
    INode GetParallelTree(string expression);
}