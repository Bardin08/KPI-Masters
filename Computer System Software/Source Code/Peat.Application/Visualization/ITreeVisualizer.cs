using Peat.Core.Syntax.Nodes;

namespace Peat.Application.Visualization;

public interface ITreeVisualizer
{
    string Visualize(INode tree);
}