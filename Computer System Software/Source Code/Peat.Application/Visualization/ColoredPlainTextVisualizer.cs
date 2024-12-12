using System.Text;
using Peat.Core.Syntax.Nodes;

namespace Peat.Application.Visualization;

public class ColoredPlainTextTreeVisualizer : ITreeVisualizer
{
    private const string Pipe = "│   ";
    private const string Tee = "├── ";
    private const string LastTee = "└── ";
    
    public string Visualize(INode tree)
    {
        var builder = new StringBuilder();
        PrintNode(builder, tree, "", true);
        return builder.ToString();
    }

    private void PrintNode(StringBuilder sb, INode node, string indent, bool isLast)
    {
        // Print current node
        sb.Append(indent);
        sb.Append(isLast ? LastTee : Tee);

        string nodeText = node switch
        {
            BinaryNode b => $"\e[33m{b.Operator}\e[0m",  // Yellow for operators
            NumberNode n => $"\e[36m{n.Value}\e[0m",     // Cyan for numbers
            VariableNode v => $"\e[32m{v.Name}\e[0m",    // Green for variables
            FunctionNode f => $"\e[35m{f.Name}\e[0m",    // Magenta for functions
            _ => "Unknown"
        };

        sb.AppendLine(nodeText);

        // Print children
        var newIndent = indent + (isLast ? "    " : Pipe);
        
        switch (node)
        {
            case BinaryNode b:
                PrintNode(sb, b.Left, newIndent, false);
                PrintNode(sb, b.Right, newIndent, true);
                break;
            
            case FunctionNode f:
                var lastIndex = f.Arguments.Count - 1;
                for (int i = 0; i < f.Arguments.Count; i++)
                {
                    PrintNode(sb, f.Arguments[i], newIndent, i == lastIndex);
                }
                break;
        }
    }
}