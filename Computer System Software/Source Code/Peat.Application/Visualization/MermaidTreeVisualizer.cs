using System.Text;
using Peat.Core.Syntax.Nodes;

namespace Peat.Application.Visualization;

public class MermaidTreeVisualizer : ITreeVisualizer
{
    private int _nodeCounter;
    private readonly StringBuilder _mermaid = new();

    public string Visualize(INode tree)
    {
        _nodeCounter = 0;
        _mermaid.Clear();
       
        _mermaid.AppendLine("graph TD");
        VisualizeNode(tree);
       
        return _mermaid.ToString();
    }

    private void VisualizeNode(INode node, string? parentId = null)
    {
        var currentId = $"node{++_nodeCounter}";

        // Escape special characters for operators
        var label = GetNodeLabel(node);
        var escapedLabel = label switch
        {
            "+" => "plus",
            "-" => "minus", 
            "*" => "mul",
            "/" => "div",
            _ => label
        };

        _mermaid.AppendLine($"    {currentId}[\"{escapedLabel}\"]");
       
        if (parentId != null)
        {
            _mermaid.AppendLine($"    {parentId} --> {currentId}");
        }

        switch (node)
        {
            case BinaryNode binary:
                VisualizeNode(binary.Left, currentId);
                VisualizeNode(binary.Right, currentId);
                break;
               
            case FunctionNode func:
                foreach (var arg in func.Arguments)
                {
                    VisualizeNode(arg, currentId);
                }
                break;
        }
    }

    private string GetNodeLabel(INode node) => node switch
    {
        BinaryNode b => b.Operator,
        NumberNode n => n.Value,
        VariableNode v => v.Name,
        FunctionNode f => f.Name,
        _ => "Unknown"
    };
}