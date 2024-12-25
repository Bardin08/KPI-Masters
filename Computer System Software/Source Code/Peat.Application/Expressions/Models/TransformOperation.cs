using Peat.Core.Lexer;
using Peat.Core.Syntax.Nodes;

namespace Peat.Application.Expressions.Models;

public record TransformOperation(
    string Type,
    string Description,
    IReadOnlyList<Token> OriginalTokens,
    IReadOnlyList<Token> ResultTokens,
    INode? OriginalNode,
    INode? ResultNode
);