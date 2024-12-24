using Peat.Core.Lexer;
using Peat.Core.Optimization.Common;

namespace Peat.Core.Optimization
{
    public class UnaryOperatorsOptimizer : BaseOptimizer
    {
        private readonly HashSet<string> _unaryOperators = ["+", "-"];

        public override IEnumerable<Token> Optimize(IEnumerable<Token> tokens)
        {
            var tokenList = tokens.ToList();
            var result = ProcessTokens(tokenList);
            return result.Select((t, idx) => t with { Position = idx + 1 });
        }

        private List<Token> ProcessTokens(List<Token> tokens)
        {
            var result = new List<Token>();
            var position = 0;

            while (position < tokens.Count)
            {
                try
                {
                    if (IsUnaryOperator(tokens, position))
                    {
                        var (nextPosition, optimizedTokens, originalTokens) = HandleUnaryOperation(tokens, position);
                        RecordTransformation(originalTokens, optimizedTokens);
                        result.AddRange(optimizedTokens);
                        position = nextPosition;
                    }
                    else
                    {
                        result.Add(tokens[position]);
                        position++;
                    }
                }
                catch (Exception ex) when (ex is InvalidOperationException or NotSupportedException)
                {
                    throw new OptimizerException($"Error at position {position}: {ex.Message}", ex);
                }
            }

            return result;
        }

        private (int nextPosition, List<Token> optimized, List<Token> original) HandleUnaryOperation(
            List<Token> tokens, int startPosition)
        {
            var operatorTokens = CollectOperators(tokens, startPosition, out var operandPosition);
            var operand = CollectOperand(tokens, operandPosition, out var nextPosition);
            var originalTokens = operatorTokens.Concat(operand).ToList();
            
            return (nextPosition, ApplyOperators(operatorTokens, operand), originalTokens);
        }

        private void RecordTransformation(List<Token> original, List<Token> result)
        {
            TransformationContext.RecordTransformation(
                "UnaryOperatorOptimization",
                "Optimized unary operator expression",
                original,
                result,
                null, // Node transformations not implemented
                null
            );
        }

        private List<Token> CollectOperators(List<Token> tokens, int position, out int nextPosition)
        {
            var operators = new List<Token>();
            nextPosition = position;

            while (nextPosition < tokens.Count && IsUnaryOperator(tokens, nextPosition))
            {
                operators.Add(tokens[nextPosition++]);
            }

            if (nextPosition >= tokens.Count)
                throw new InvalidOperationException("Expression ends with unary operator");

            return operators;
        }

        private List<Token> CollectOperand(List<Token> tokens, int position, out int nextPosition)
        {
            return tokens[position] switch
            {
                { TokenType: TokenType.LParen } => CollectParenthesizedExpression(tokens, position, out nextPosition),
                { TokenType: TokenType.Function } => CollectFunctionExpression(tokens, position, out nextPosition),
                _ => CollectSingleToken(tokens, position, out nextPosition)
            };
        }

        private List<Token> CollectSingleToken(List<Token> tokens, int position, out int nextPosition)
        {
            nextPosition = position + 1;
            return new List<Token> { tokens[position] };
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

        private List<Token> CollectFunctionExpression(List<Token> tokens, int position, out int nextPosition)
        {
            var functionName = tokens[position].Value;
            nextPosition = position + 1;

            if (nextPosition >= tokens.Count || tokens[nextPosition].Value != "(")
                throw new InvalidOperationException("Function must be followed by parenthesis");

            return new List<Token> { new Token(TokenType.Function, -1, $"{functionName}(") }
                .Concat(CollectParenthesizedExpression(tokens, nextPosition, out nextPosition))
                .ToList();
        }

        private List<Token> ApplyOperators(List<Token> operators, List<Token> operand)
        {
            var result = Optimize(operand).ToList();

            foreach (var op in operators.AsEnumerable().Reverse())
            {
                result = op.Value switch
                {
                    "-" => WrapWithZeroMinus(result),
                    "+" => result,
                    _ => throw new NotSupportedException($"Unsupported operator: {op.Value}")
                };
            }

            return result;
        }

        private List<Token> WrapWithZeroMinus(List<Token> operand) => new List<Token>
        {
            new(TokenType.LParen, -1, "("),
            new(TokenType.Number, -1, "0"),
            new(TokenType.Operator, -1, "-")
        }.Concat(operand).Append(new Token(TokenType.RParen, -1, ")")).ToList();

        private bool IsUnaryOperator(List<Token> tokens, int position) =>
            tokens[position] is { TokenType: TokenType.Operator } token &&
            _unaryOperators.Contains(token.Value) &&
            (position == 0 || IsUnaryContext(tokens[position - 1]));

        private bool IsUnaryContext(Token token) =>
            token.TokenType is TokenType.Operator or TokenType.Comma ||
            (token.TokenType is TokenType.LParen && token.Value == "(");
    }
}