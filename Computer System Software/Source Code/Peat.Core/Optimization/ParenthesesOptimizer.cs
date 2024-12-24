using Peat.Core.Lexer;

namespace Peat.Core.Optimization
{
    public class ParenthesesOptimizer : BaseOptimizer
    {
        public override IEnumerable<Token> Optimize(IEnumerable<Token> tokens)
        {
            var tokenList = tokens.ToList();
            return OptimizeRecursively(tokenList).Select((t, idx) => t with { Position = idx + 1 });
        }

        private List<Token> OptimizeRecursively(List<Token> tokens)
        {
            var result = new List<Token>();
            var position = 0;

            while (position < tokens.Count)
            {
                if (IsRemovableParentheses(tokens, position))
                {
                    var (nextPosition, optimizedTokens, originalTokens) = HandleParentheses(tokens, position);
                    RecordTransformation(originalTokens, optimizedTokens);
                    result.AddRange(OptimizeRecursively(optimizedTokens));
                    position = nextPosition;
                }
                else
                {
                    result.Add(tokens[position]);
                    position++;
                }
            }

            return result;
        }

        private (int nextPosition, List<Token> optimized, List<Token> original) HandleParentheses(
            List<Token> tokens, int startPosition)
        {
            var parenContent = CollectParenthesizedExpression(tokens, startPosition, out var nextPosition);
            var optimizedContent = RemoveRedundantParentheses(parenContent);
            
            return (nextPosition, optimizedContent, parenContent);
        }

        private List<Token> CollectParenthesizedExpression(List<Token> tokens, int position, out int nextPosition)
        {
            var result = new List<Token>();
            var depth = 0;
            nextPosition = position;

            do
            {
                if (nextPosition >= tokens.Count)
                    throw new InvalidOperationException("Unclosed parenthesis");

                var token = tokens[nextPosition++];
                result.Add(token);

                depth += token.Value switch
                {
                    "(" => 1,
                    ")" => -1,
                    _ => 0
                };
            } while (depth > 0);

            return result;
        }

        private List<Token> RemoveRedundantParentheses(List<Token> tokens)
        {
            if (tokens.Count < 3) return tokens;
            return tokens.Skip(1).SkipLast(1).ToList();
        }

        private bool IsRemovableParentheses(List<Token> tokens, int position)
        {
            if (position >= tokens.Count || tokens[position].TokenType != TokenType.LParen)
                return false;

            return !IsPrevToken(tokens, position, TokenType.Function);
        }

        private void RecordTransformation(List<Token> original, List<Token> result)
        {
            if (original.SequenceEqual(result)) return;
            
            TransformationContext.RecordTransformation(
                "ParenthesesOptimization",
                "Removed redundant parentheses",
                original,
                result,
                null,
                null
            );
        }
    }
}