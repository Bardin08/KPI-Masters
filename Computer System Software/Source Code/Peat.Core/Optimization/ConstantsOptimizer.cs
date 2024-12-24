using System.Globalization;
using Peat.Core.Lexer;

namespace Peat.Core.Optimization;

public class ConstantsOptimizer : BaseOptimizer
{
    public override IEnumerable<Token> Optimize(IEnumerable<Token> tokens)
    {
        var tokenList = tokens.ToList();
        var index = 0;
        var result = ProcessTokens(tokenList, ref index);
        // Final fold to ensure maximum simplification
        return FoldConstantsUntilNoChange(result);
    }

    private List<Token> ProcessTokens(List<Token> tokens, ref int index)
    {
        var result = new List<Token>();

        while (index < tokens.Count)
        {
            var current = tokens[index];
            switch (current.TokenType)
            {
                case TokenType.Function:
                    ProcessFunction(tokens, ref index, result);
                    break;

                case TokenType.LParen:
                    ProcessParentheses(tokens, ref index, result);
                    break;

                case TokenType.RParen:
                    // If we hit a right parenthesis, return the folded subexpression to the caller
                    index++;
                    return FoldConstantsUntilNoChange(result);

                default:
                    // Just add the current token and move on
                    result.Add(current);
                    index++;
                    break;
            }
        }

        return FoldConstantsUntilNoChange(result);
    }

    private void ProcessFunction(List<Token> tokens, ref int index, List<Token> result)
    {
        // Add function name
        result.Add(tokens[index++]);

        // Expect '(' after function name
        if (index < tokens.Count && tokens[index].TokenType == TokenType.LParen)
        {
            // Add '('
            result.Add(tokens[index++]);

            // Process function arguments until ')'
            var args = ProcessTokens(tokens, ref index);

            // 'ProcessTokens' returns on 'RParen' but doesn't add it
            // Add the folded arguments
            result.AddRange(args);

            // Now we must add the closing ')'
            result.Add(new Token(TokenType.RParen, tokens[index - 1].Position, ")"));
        }
    }

    private void ProcessParentheses(List<Token> tokens, ref int index, List<Token> result)
    {
        index++; // Skip LParen
        var subExpr = ProcessTokens(tokens, ref index);

        // subExpr is already folded inside ProcessTokens at the end, but let's ensure again
        subExpr = FoldConstantsUntilNoChange(subExpr);

        // If the subexpression inside parentheses is a single number, 
        // we don't need parentheses anymore.
        if (subExpr.Count == 1 && subExpr[0].TokenType == TokenType.Number)
        {
            result.Add(subExpr[0]);
        }
        else
        {
            // Re-add parentheses
            result.Add(new Token(TokenType.LParen, 0, "("));
            result.AddRange(subExpr);
            result.Add(new Token(TokenType.RParen, 0, ")"));
        }
    }

    /// <summary>
    /// Repeatedly calls FoldConstants until no changes occur, ensuring maximum simplification.
    /// </summary>
    private List<Token> FoldConstantsUntilNoChange(List<Token> tokens)
    {
        while (true)
        {
            var folded = FoldConstantsOnce(tokens);
            if (folded.Count == tokens.Count && folded.SequenceEqual(tokens))
                return folded;
            tokens = folded;
        }
    }

    /// <summary>
    /// Perform one pass of folding (Number Operator Number) -> Number.
    /// If multiple operations are present, it only folds them left-to-right once.
    /// Another call might fold further if results chain together.
    /// </summary>
    private List<Token> FoldConstantsOnce(List<Token> tokens)
    {
        var result = new List<Token>();

        for (var i = 0; i < tokens.Count; i++)
        {
            // Look ahead for a pattern: Number Operator Number
            if (i + 2 < tokens.Count &&
                tokens[i].TokenType == TokenType.Number &&
                tokens[i + 1].TokenType == TokenType.Operator &&
                tokens[i + 2].TokenType == TokenType.Number)
            {
                result.Add(ComputeOperation(tokens[i], tokens[i + 1], tokens[i + 2]));
                i += 2; // Skip the next two tokens as they are consumed
            }
            else
            {
                result.Add(tokens[i]);
            }
        }

        return result;
    }

    private Token ComputeOperation(Token left, Token op, Token right)
    {
        var leftValue = double.Parse(left.Value, CultureInfo.InvariantCulture);
        var rightValue = double.Parse(right.Value, CultureInfo.InvariantCulture);

        var computation = op.Value switch
        {
            "+" => leftValue + rightValue,
            "-" => leftValue - rightValue,
            "*" => leftValue * rightValue,
            "/" when rightValue != 0 => leftValue / rightValue,
            "/" => throw new DivideByZeroException("Division by zero encountered."),
            _ => throw new ArgumentException($"Unknown operator: {op.Value}")
        };

        return new Token(TokenType.Number, left.Position, computation.ToString(CultureInfo.InvariantCulture));
    }
}