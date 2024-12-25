using Peat.Core.Syntax.Nodes;

namespace Peat.Application.Expressions.Models;

public record ParallelTreeResponse
{
    public required INode Tree { get; init; }
    public required ProcessingContext Context { get; init; }
    public required TimeSpan ProcessingTime { get; init; }
}