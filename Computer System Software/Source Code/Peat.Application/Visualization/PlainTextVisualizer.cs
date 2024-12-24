using System.Text;
using Peat.Core.Syntax.Nodes;

namespace Peat.Application.Visualization;

public class PlainTextVisualizer : ITreeVisualizer
{
    private readonly StringBuilder _stringBuilder = new();

    public string Visualize(INode root)
    {
        _stringBuilder.Clear();
        PrintNode(root, "", false);
        return _stringBuilder.ToString();
    }

    private void PrintNode(INode node, string indent, bool isLeft)
    {
        var prefix = indent + (isLeft ? "├── " : "└── ");

        switch (node)
        {
            case BinaryNode b:
                _stringBuilder.AppendLine(prefix + b.Operator);
                PrintNode(b.Left, indent + (isLeft ? "│   " : "    "), true);
                PrintNode(b.Right, indent + (isLeft ? "│   " : "    "), false);
                break;

            case VariableNode v:
                _stringBuilder.AppendLine(prefix + v.Name);
                break;

            case FunctionNode f:
                _stringBuilder.AppendLine(prefix + f.Name + "(...)");
                for (var i = 0; i < f.Arguments.Count; i++)
                {
                    var argIsLeft = i < f.Arguments.Count - 1;
                    PrintNode(f.Arguments[i], indent + (isLeft ? "│   " : "    "), argIsLeft);
                }

                break;

            case NumberNode n:
                _stringBuilder.AppendLine(prefix + n.Value);
                break;

            default:
                _stringBuilder.AppendLine(prefix + node.GetType().Name);
                break;
        }
    }
}