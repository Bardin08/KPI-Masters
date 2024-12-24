namespace Peat.Core.Optimization.Common;

public interface ITransformationReporter
{
    string GenerateReport(TransformationContext context);
    string GeneratePlainReport(TransformationContext context);
}