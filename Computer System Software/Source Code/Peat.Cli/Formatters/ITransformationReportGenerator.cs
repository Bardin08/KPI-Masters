using Peat.Cli.Commands.Models;
using Peat.Core.Optimization.Common;

namespace Peat.Cli;

public interface ITransformationReportGenerator
{
    string GenerateReport(OptimizationStepViewModel context);
    string GeneratePlainReport(TransformationContext context);
}