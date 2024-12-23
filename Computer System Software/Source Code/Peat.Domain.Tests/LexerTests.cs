using Peat.Core.Lexer;

namespace Peat.Domain.Tests;

public class LexerTests
{
    [Fact]
    public void ComplexExpression_TokenizesCorrectly()
    {
        var input = "sin(x + 2.5)";
        var expectedTokens = new[]
        {
            (TokenType.Function, "sin"),
            (TokenType.LParen, "("),
            (TokenType.Variable, "x"),
            (TokenType.Operator, "+"),
            (TokenType.Number, "2.5"),
            (TokenType.RParen, ")"),
            (TokenType.Eof, "EOF")
        };

        var lexer = new Lexer();
        var tokens = lexer.Tokenize(input).ToList();

        Assert.Equal(expectedTokens.Length, tokens.Count);
        for (var i = 0; i < expectedTokens.Length; i++)
        {
            Assert.Equal(expectedTokens[i].Item1, tokens[i].TokenType);
            Assert.Equal(expectedTokens[i].Item2, tokens[i].Value);
        }
    }

    [Fact]
    public void InvalidInput_ThrowsLexerException()
    {
        var input = "2 + @ + 3";
        var lexer = new Lexer();

        try
        {
            _ = lexer.Tokenize(input).ToList();
            Assert.Fail();
        }
        catch (LexerException ex)
        {
            Assert.Contains("Invalid token '@'", ex.Message);
        }
    }
}