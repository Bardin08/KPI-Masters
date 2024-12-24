using Peat.Core.Lexer;
using Peat.Core.Optimization;

namespace Peat.Domain.Tests.Optimization;

public class ConstantsOptimizerTests
{
    private readonly Lexer _lexer = new();
    private readonly ConstantsOptimizer _optimizer = new();

    [Theory]
    [InlineData("1 + 2", "3")]
    [InlineData("4 - 2", "2")]
    [InlineData("3 * 4", "12")]
    [InlineData("8 / 2", "4")]
    [InlineData("2 + 3 + 4", "9")] // Multiple operations
    [InlineData("2 * 3 * 4", "24")] // Multiple multiplications
    public void Should_Fold_Simple_Arithmetic(string input, string expected)
    {
        AssertOptimization(input, expected);
    }

    [Theory]
    [InlineData("1 + a", "1 + a")]
    [InlineData("2 * x", "2 * x")]
    [InlineData("a + 1 + 2", "a + 3")]
    [InlineData("1 + 2 + x", "3 + x")]
    public void Should_Preserve_Variables(string input, string expected)
    {
        AssertOptimization(input, expected);
    }

    [Theory]
    [InlineData("(1 + 2) * 3", "9")]
    [InlineData("(2 + 3) + (4 + 5)", "14")]
    [InlineData("(1 + 2) * (3 + 4)", "21")]
    public void Should_Fold_Nested_Expressions(string input, string finalResult)
    {
        // First pass should fold within parentheses
        var firstPass = _optimizer.Optimize(_lexer.Tokenize(input));

        // Second pass should fold the entire expression
        var secondPass = _optimizer.Optimize(firstPass);
        AssertTokensEqual(_lexer.Tokenize(finalResult), secondPass);
    }

    [Theory]
    [InlineData("1 + sin(2 + 3)", "1 + sin(5)")]
    [InlineData("cos(1 + 2) + sin(3 + 4)", "cos(3) + sin(7)")]
    public void Should_Fold_Inside_Functions(string input, string expected)
    {
        AssertOptimization(input, expected);
    }

    [Theory]
    [InlineData("2/0")]
    [InlineData("1 + (2/0)")]
    public void Should_Preserve_Division_By_Zero(string input)
    {
        var tokens = _lexer.Tokenize(input).ToList();
        Assert.Throws<DivideByZeroException>(() => _optimizer.Optimize(tokens));
    }

    [Theory]
    [InlineData("1 + 2", "3")] // Floating point precision test
    [InlineData("0 - 1 + 0 - 2", "-3")] // Negative numbers
    [InlineData("1.5 * 2", "3")] // Mixed numbers
    public void Should_Handle_Special_Numbers(string input, string expected)
    {
        AssertOptimization(input, expected);
    }

    [Theory]
    [InlineData("(1 + 2) * 3", "9")]
    [InlineData("3 * (1 + 2)", "9")]
    [InlineData("(2 * 3) / (2 + 2)", "1.5")]
    public void Should_Fold_Mixed_Expressions(string input, string finalResult)
    {
        AssertOptimization(input, finalResult);
    }

    private void AssertOptimization(string input, string expected)
    {
        var result = _optimizer.Optimize(_lexer.Tokenize(input));
        AssertTokensEqual(_lexer.Tokenize(expected), result);
    }

    private void AssertTokensEqual(IEnumerable<Token> expected, IEnumerable<Token> actual)
    {
        var expectedList = expected.ToList();
        var actualList = actual.ToList();

        Assert.Equal(
            expectedList.Select(t => t.Value),
            actualList.Select(t => t.Value));
    }
}