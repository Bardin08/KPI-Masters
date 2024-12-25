namespace Peat.Cli.Commands.Models;

public record TransformationStepViewModel(
    Guid Id,
    DateTime Timestamp,
    TransformOperationViewModel Operation,
    TransformPositionViewModel Position
);