namespace Peat.Application.Expressions.Models.Mappers;

internal static class Mappers
{
    public static TransformationStep ToApplicationModel(this Core.Optimization.Common.TransformationStep s) =>
        new(s.Id, s.Timestamp, s.Operation.ToApplicationModel(), s.Position.ToApplicationModel());

    public static TransformPosition ToApplicationModel(this Core.Optimization.Common.TransformPosition p) =>
        new(p.StartIndex, p.EndIndex, p.OriginalFragment);

    public static TransformOperation ToApplicationModel(this Core.Optimization.Common.TransformOperation op) =>
        new(
            op.Type,
            op.Description,
            op.OriginalTokens,
            op.ResultTokens,
            op.OriginalNode,
            op.ResultNode
        );
}