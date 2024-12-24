using Peat.Core.Lexer;
using Peat.Core.Optimization;

namespace Peat.Domain.Tests.Optimization;

public class UnaryOperatorsOptimizationTests
{
    private readonly Lexer _lexer = new();
    private readonly UnaryOperatorsOptimizer _optimizer = new();

    [Theory]
    [InlineData("-a", "(0 - a)")]
    [InlineData("+a", "a")]
    [InlineData("a + -b", "a + (0 - b)")]
    [InlineData("a + +b", "a + b")]
    [InlineData("(-a)", "((0 - a))")]
    [InlineData("(+a)", "(a)")]
    [InlineData("sin(-x)", "sin((0 - x))")]
    [InlineData("sin(+x)", "sin(x)")]
    [InlineData("sin(-x, +y)", "sin((0 - x), y)")]
    [InlineData("a + -b + -c", "a + (0 - b) + (0 - c)")]
    [InlineData("a * -b", "a * (0 - b)")]
    [InlineData("a/-b", "a/(0 - b)")]
    [InlineData("-1", "(0 - 1)")]
    [InlineData("+1", "1")]
    [InlineData("-(a+b)", "(0 - (a + b))")]
    [InlineData("+(a+b)", "(a + b)")]
    [InlineData("-a * b + -c", "(0 - a) * b + (0 - c)")]
    [InlineData("-a + b * -c", "(0 - a) + b * (0 - c)")]
    [InlineData("a * -b + c * -d", "a * (0 - b) + c * (0 - d)")]
    [InlineData("-(-(a+b))", "(0 - ((0 - (a + b))))")]
    [InlineData("-(-(-a))", "(0 - ((0 - ((0 - a)))))")]
    [InlineData("-(-a * -b)", "(0 - ((0 - a) * (0 - b)))")]
    [InlineData("sin(-a * -b)", "sin((0 - a) * (0 - b))")]
    [InlineData("sin(-a + -b)", "sin((0 - a) + (0 - b))")]
    [InlineData("sin(cos(-x))", "sin(cos((0 - x)))")]
    [InlineData("- a", "(0 - a)")]
    [InlineData("a *- b", "a * (0 - b)")]
    [InlineData("a +- b", "a + (0 - b)")]
    public void Should_Handle_Unary_Operators(string input, string expected)
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
    [InlineData("(a+b)*c")]
    public void Should_Not_Modify_Expressions_Without_Unary_Operators(string input)
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

    [Theory]
    [InlineData("--a")] // Double negation
    [InlineData("+-a")] // Plus followed by minus
    [InlineData("-+a")] // Minus followed by plus
    [InlineData("++a")] // Double plus
    public void Should_Handle_Multiple_Sequential_Unary_Operators(string input)
    {
        var inputTokens = _lexer.Tokenize(input);

        var result = _optimizer.Optimize(inputTokens).ToList();

        Assert.All(result, token =>
            Assert.True(token.TokenType != TokenType.Operator ||
                        (token.Value != "+" && token.Value != "-") ||
                        result.IndexOf(token) > 0 &&
                        result[result.IndexOf(token) - 1].TokenType != TokenType.Operator));
    }
}