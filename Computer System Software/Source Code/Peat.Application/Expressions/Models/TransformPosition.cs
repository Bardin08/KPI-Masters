namespace Peat.Application.Expressions.Models;

public record TransformPosition(
    int StartIndex,
    int EndIndex,
    string OriginalFragment
);