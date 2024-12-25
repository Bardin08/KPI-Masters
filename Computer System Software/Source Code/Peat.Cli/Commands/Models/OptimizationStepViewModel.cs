namespace Peat.Cli.Commands.Models;

public record OptimizationStepViewModel(
    string Name,
    int TokensBefore,
    int TokensAfter,
    List<TransformationStepViewModel>? TransformationSteps);