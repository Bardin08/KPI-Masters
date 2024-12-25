using Peat.Cli.Commands.Models;

namespace Peat.Cli.Formatters;

public interface IOutputFormatter
{
    public string Format(VisualizationData data, bool verbose);    
}