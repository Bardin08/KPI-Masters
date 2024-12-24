using Moq;
using Peat.Application.Expressions.Models;
using Peat.Application.Expressions.Services;
using Peat.Core.Lexer;
using Peat.Core.Syntax.Parser;

namespace Peat.Application.Tests;

public class ExpressionServiceTests
{
    private readonly IExpressionService _service;
    private readonly Mock<ILexer> _lexerMock;
    private readonly Mock<IParser> _parserMock;

    public ExpressionServiceTests()
    {
        _lexerMock = new Mock<ILexer>();
        _parserMock = new Mock<IParser>();
        _service = new ExpressionService(_lexerMock.Object, _parserMock.Object);
    }

    [Fact]
    public void ValidateAsync_ValidExpression_ReturnsSuccess()
    {
        var expression = "1 + 2";
        _lexerMock.Setup(l => l.Tokenize(expression))
            .Returns([new Token(TokenType.Number, 0, "1")]);

        var result = _service.Validate(expression);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void ValidateAsync_LexerError_ReturnsLexicalError()
    {
        var expr = "1 +* 2";
        _lexerMock.Setup(l => l.Tokenize(expr))
            .Throws(new LexerException([new LexerError("Invalid token", 2)]));

        var result = _service.Validate(expr);

        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal(ErrorType.Lexical, result.Errors[0].Type);
    }

    [Fact]
    public void ValidateAsync_ParserError_ReturnsSyntaxError()
    {
        var expr = "1 + (2";
        var tokens = new[] { new Token(TokenType.Number, 1, "1") };
        _lexerMock.Setup(l => l.Tokenize(expr)).Returns(tokens);
        _parserMock.Setup(p => p.Parse(tokens))
            .Throws(new ParserException(ParserError.UnmatchedOpeningParenthesis(new Token(TokenType.RParen, 4, "("))));

        var result = _service.Validate(expr);

        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal(ErrorType.Syntax, result.Errors[0].Type);
    }
}