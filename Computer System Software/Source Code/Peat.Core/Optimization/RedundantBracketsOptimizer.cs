using Peat.Core.Lexer;

namespace Peat.Core.Optimization;

public class RedundantBracketsOptimizer : BaseOptimizer
{
    private List<Token> _allTokens = null!;

    public override IEnumerable<Token> Optimize(IEnumerable<Token> tokens)
    {
        _allTokens = tokens.ToList();
        var resultTokens = OptimizeTokens(_allTokens);
        return resultTokens.Select((t, idx) => t with { Position = idx + 1 });
    }

    private List<Token> OptimizeTokens(List<Token> tokens)
    {
        var result = new List<Token>();
        var position = 0;

        while (position < tokens.Count)
        {
            var token = tokens[position];

            if (token.TokenType == TokenType.Function)
            {
                result.Add(tokens[position++]);
                if (position < tokens.Count && tokens[position].TokenType == TokenType.LParen)
                {
                    var (nextPos, content) = GetBracketContent(tokens, position);
                    var optimizedInner = OptimizeTokens(content.Skip(1).SkipLast(1).ToList());
                    result.Add(content[0]);
                    result.AddRange(optimizedInner);
                    result.Add(content[^1]);
                    position = nextPos;
                }
            }
            else if (token.TokenType == TokenType.LParen)
            {
                var (nextPos, content) = GetBracketContent(tokens, position);
                var inner = content.Skip(1).SkipLast(1).ToList();

                var isRedundant = IsRedundantBrackets(content);
                if (isRedundant)
                {
                    TransformationContext.RecordTransformation(
                        "RedundantBracketsOptimization",
                        "Removed redundant brackets",
                        content,
                        inner,
                        null,
                        null
                    );

                    var optimizedInner = OptimizeTokens(inner);
                    result.AddRange(optimizedInner);
                }
                else
                {
                    var optimizedInner = OptimizeTokens(inner);
                    result.Add(content[0]);
                    result.AddRange(optimizedInner);
                    result.Add(content[^1]);
                }

                position = nextPos;
            }
            else
            {
                result.Add(token);
                position++;
            }
        }

        return result;
    }

    private bool IsRedundantBrackets(List<Token> tokens)
    {
        var inner = tokens.Skip(1).SkipLast(1).ToList();

        if (inner.Count == 1)
            return true;

        if (inner.All(t => t.TokenType != TokenType.Operator))
            return GetBracketDepth(inner) == 0;

        var innerOps = inner.Where(t => t.TokenType == TokenType.Operator).ToList();
        var startIndex = _allTokens.FindIndex(t => t == tokens.First());
        var endIndex = _allTokens.FindIndex(t => t == tokens.Last());

        var prevOp = _allTokens.Take(startIndex)
            .LastOrDefault(t => t.TokenType == TokenType.Operator);
        var nextOp = _allTokens.Skip(endIndex + 1)
            .FirstOrDefault(t => t.TokenType == TokenType.Operator);

        foreach (var innerOp in innerOps)
        {
            var innerPrecedence = GetOperatorPrecedence(innerOp.Value);
            var shouldKeepForPrevOp = prevOp != null && GetOperatorPrecedence(prevOp.Value) > innerPrecedence;
            var shouldKeepForNextOp = nextOp != null && GetOperatorPrecedence(nextOp.Value) > innerPrecedence;

            if (!shouldKeepForPrevOp && !shouldKeepForNextOp)
                continue;

            TransformationContext.RecordTransformation(
                "RedundantBracketsOptimization",
                $"Keeping brackets due to operator precedence: inner={innerOp.Value} " +
                $"prev={prevOp?.Value} next={nextOp?.Value}",
                tokens,
                tokens,
                null,
                null
            );
            return false;
        }

        return true;
    }

    private int GetOperatorPrecedence(string op) => op switch
    {
        "*" or "/" => 2,
        "+" or "-" => 1,
        _ => 0
    };

    private int GetBracketDepth(List<Token> tokens)
    {
        var depth = 0;
        foreach (var token in tokens)
        {
            if (token.TokenType == TokenType.LParen) depth++;
            else if (token.TokenType == TokenType.RParen) depth--;
        }

        return depth;
    }

    private (int nextPos, List<Token> content) GetBracketContent(List<Token> tokens, int start)
    {
        var depth = 1;
        var pos = start + 1;
        var content = new List<Token> { tokens[start] };

        while (pos < tokens.Count && depth > 0)
        {
            var token = tokens[pos++];
            content.Add(token);

            if (token.TokenType == TokenType.LParen) depth++;
            else if (token.TokenType == TokenType.RParen) depth--;
        }

        return (pos, content);
    }
}