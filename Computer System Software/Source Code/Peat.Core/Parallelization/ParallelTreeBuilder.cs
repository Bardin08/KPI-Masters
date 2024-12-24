using Peat.Core.Syntax.Nodes;

namespace Peat.Core.Parallelization;

public class ParallelTreeBuilder : IParallelTreeBuilder
{
    private readonly Dictionary<INode, ParallelNode> _nodeMap = new();
    private int _currentOrder;

    public ParallelNode Build(INode ast)
    {
        _nodeMap.Clear();
        _currentOrder = 0;
        
        // Build the graph without assigning levels
        BuildNode(ast);
        AssignLevels();

        return _nodeMap[ast];
    }

    private ParallelNode BuildNode(INode node)
    {
        if (_nodeMap.TryGetValue(node, out var existing))
            return existing;

        var dependencies = node switch
        {
            BinaryNode b => [BuildNode(b.Left), BuildNode(b.Right)],
            FunctionNode f => f.Arguments.Select(BuildNode),
            _ => []
        };

        var parallelNode = new ParallelNode
        {
            OriginalNode = node,
            Level = 0, // temporary, will assign after building
            Order = _currentOrder++,
            Dependencies = dependencies.ToList()
        };

        _nodeMap[node] = parallelNode;
        return parallelNode;
    }

    private void AssignLevels()
    {
        // Topological level assignment:
        // Level of a node = max(Dependencies.Level) + 1, or 0 if no dependencies.

        // Build a reverse dependency map: which nodes depend on which node?
        // Actually, we just have a forward dependency from node to Dependencies.
        // We need to process nodes in order of their dependencies being assigned.

        var incomingEdgesCount = new Dictionary<ParallelNode, int>();
        foreach (var node in _nodeMap.Values)
        {
            incomingEdgesCount[node] = 0; 
        }

        foreach (var node in _nodeMap.Values)
        {
            // For each dependency, increment its dependent's incoming edge count
            foreach (var unused in node.Dependencies)
            {
                incomingEdgesCount[node] += 1;
            }
        }

        var noDepsNodes = new Queue<ParallelNode>(
            _nodeMap.Values.Where(n => incomingEdgesCount[n] == 0));

        // Assign level = 0 to nodes with no dependencies
        while (noDepsNodes.Count > 0)
        {
            var current = noDepsNodes.Dequeue();

            // current.Level is max of deps + 1, but if no deps, stays 0
            current.Level = current.Dependencies.Any()
                ? current.Dependencies.Max(d => d.Level) + 1
                : 0;

            // Decrease incoming edge count for nodes that depend on 'current'
            var currentNodesDependents = _nodeMap.Values
                .Where(node => node.Dependencies.Contains(current));

            foreach (var node in currentNodesDependents)
            {
                incomingEdgesCount[node] -= 1;
                if (incomingEdgesCount[node] == 0)
                    noDepsNodes.Enqueue(node);
            }
        }
    }

    public ParallelizationMetrics GetMetrics()
    {
        var levels = _nodeMap.Values
            .GroupBy(n => n.Level)
            .OrderBy(x => x.Key)
            .ToList();

        return new ParallelizationMetrics
        {
            Height = levels.Count,
            MaxWidth = levels.Max(l => l.Count()),
            TotalNodes = _nodeMap.Count,
            LevelDistribution = levels.ToDictionary(g => g.Key, g => g.Count())
        };
    }
}