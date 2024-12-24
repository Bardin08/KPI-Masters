using Peat.Core.Lexer;
using Peat.Core.Optimization;

namespace Peat.Domain.Tests.Optimization
{
    public class RedundantBracketsOptimizerTests
    {
        private readonly Lexer _lexer = new();
        private readonly RedundantBracketsOptimizer _optimizer = new();

        [Theory]
        [InlineData("(a)", "a")]
        [InlineData("((a))", "a")]
        [InlineData("(1)", "1")]
        [InlineData("(x)", "x")]
        [InlineData("a + (b)", "a + b")]
        [InlineData("a * (b + c)", "a * (b + c)")]
        [InlineData("(a + b) * c", "(a + b) * c")]
        [InlineData("a + (b + c)", "a + b + c")]
        [InlineData("(a + b)", "a + b")]
        [InlineData("((a + b))", "a + b")]
        [InlineData("sin((x))", "sin(x)")]
        [InlineData("sin(x)", "sin(x)")]  // Don't touch function brackets
        [InlineData("(sin(x))", "sin(x)")]
        public void Should_Handle_Brackets_Correctly(string input, string expected)
        {
            // Arrange
            var inputTokens = _lexer.Tokenize(input);
            var expectedTokens = _lexer.Tokenize(expected);

            // Act
            var result = _optimizer.Optimize(inputTokens);

            // Assert
            Assert.Equal(
                string.Join(" ", expectedTokens.Select(t => t.Value)),
                string.Join(" ", result.Select(t => t.Value)));
        }

        [Theory]
        [InlineData("a+b")]
        [InlineData("1+2")]
        [InlineData("sin(x)")]
        public void Should_Not_Modify_Expressions_Without_Redundant_Brackets(string input)
        {
            // Arrange
            var inputTokens = _lexer.Tokenize(input).ToList();

            // Act
            var result = _optimizer.Optimize(inputTokens);

            // Assert
            Assert.Equal(
                string.Join("", inputTokens.Select(t => t.Value)),
                string.Join("", result.Select(t => t.Value))
            );
        }
    }
}