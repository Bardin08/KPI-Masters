using Peat.Core.Syntax.Nodes;

namespace Peat.Core.Parallelization;

public interface IParallelTreeBuilder
{
    ParallelNode Build(INode ast);
    ParallelizationMetrics GetMetrics();
}