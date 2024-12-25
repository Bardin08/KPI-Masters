using Peat.Application.Expressions.Models;

namespace Peat.Cli.Commands.Models.Mappers;

public static class Mappers
{
    public static OptimizationStepViewModel ToViewModel(this OptimizationStep s) =>
        new(
            s.OptimizerName,
            s.InputTokens.Count,
            s.OutputTokens.Count,
            s.Transformations?.Select(t => t.ToViewModel()).ToList() ?? []
        );

    public static TransformationStepViewModel ToViewModel(this TransformationStep s) =>
        new(s.Id, s.Timestamp, s.Operation.ToViewModel(), s.Position.ToViewModel());

    public static TransformPositionViewModel ToViewModel(this TransformPosition p) =>
        new(p.StartIndex, p.EndIndex, p.OriginalFragment);

    public static TransformOperationViewModel ToViewModel(this TransformOperation op) =>
        new(
            op.Type,
            op.Description,
            op.OriginalTokens,
            op.ResultTokens,
            op.OriginalNode,
            op.ResultNode
        );
}