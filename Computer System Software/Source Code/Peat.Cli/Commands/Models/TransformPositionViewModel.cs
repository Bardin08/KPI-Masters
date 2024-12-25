namespace Peat.Cli.Commands.Models;

public record TransformPositionViewModel(
    int StartIndex,
    int EndIndex,
    string OriginalFragment
);