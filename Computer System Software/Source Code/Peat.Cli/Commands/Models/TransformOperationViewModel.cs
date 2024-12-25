using Peat.Core.Lexer;
using Peat.Core.Syntax.Nodes;

namespace Peat.Cli.Commands.Models;

public record TransformOperationViewModel(
    string Type,
    string Description,
    IReadOnlyList<Token> OriginalTokens,
    IReadOnlyList<Token> ResultTokens,
    INode? OriginalNode,
    INode? ResultNode
);