using Peat.Core.Lexer;
using Peat.Core.Syntax.Nodes;
using Peat.Core.Syntax.Parser;

namespace Peat.Domain.Tests;

public class StateMachineParserTests
{
    [Theory]
    [InlineData("2 + 3", 3)] // NumberNode -> BinaryNode -> NumberNode
    [InlineData("x", 1)] // Just VariableNode
    [InlineData("sin(x)", 2)] // FunctionNode -> VariableNode
    public void Parse_ReturnsCorrectNodeCount(string input, int expectedNodes)
    {
        var lexer = new Lexer();
        var parser = new StateMachineParser();
        var ast = parser.Parse(lexer.Tokenize(input));
        Assert.Equal(expectedNodes, CountNodes(ast));
    }

    [Fact]
    public void Parse_CorrectPrecedence()
    {
        const string input = "1 + 2 * 3";
        var lexer = new Lexer();
        var parser = new StateMachineParser();
        var ast = parser.Parse(lexer.Tokenize(input));

        Assert.IsType<BinaryNode>(ast);
        var binary = (BinaryNode)ast;
        Assert.Equal("+", binary.Operator);
        Assert.IsType<NumberNode>(binary.Left);
        Assert.IsType<BinaryNode>(binary.Right);
    }

    [Fact]
    public void Parse_Function()
    {
        const string input = "sin(x + 1)";
        var lexer = new Lexer();
        var parser = new StateMachineParser();
        var ast = parser.Parse(lexer.Tokenize(input));

        Assert.IsType<FunctionNode>(ast);
        var func = (FunctionNode)ast;
        Assert.Equal("sin", func.Name);
        Assert.Single(func.Arguments);
        Assert.IsType<BinaryNode>(func.Arguments[0]);
    }

    [Theory]
    [InlineData("sin(x + 1", "Unclosed function call")]
    [InlineData("((1 + 2)", "Unmatched opening parenthesis")]
    [InlineData("1 + 2)", "Unmatched closing parenthesis")]
    public void ThrowsOnMismatchedParentheses(string input, string expectedMessage)
    {
        var parser = new StateMachineParser();
        var lexer = new Lexer();
        var tokens = lexer.Tokenize(input);
        var ex = Assert.Throws<ParserException>(() => parser.Parse(tokens));
        Assert.Contains(expectedMessage, ex.Message);
    }

    [Theory]
    [InlineData("sin")] // Missing parentheses
    [InlineData("sin()")] // Empty arguments
    [InlineData("sin(1,")] // Trailing comma
    [InlineData("sin(,1)")] // Leading comma
    public void ThrowsOnInvalidFunctionCall(string input)
    {
        var parser = new StateMachineParser();
        var lexer = new Lexer();
        var tokens = lexer.Tokenize(input);
        Assert.Throws<ParserException>(() => parser.Parse(tokens));
    }

    [Theory]
    [InlineData("1 ++ 2")] // Double operator
    [InlineData("* 1 + 2")] // Leading operator
    [InlineData("1 + 2 *")] // Trailing operator
    [InlineData("1 + (2 *)")] // Incomplete operation
    public void ThrowsOnInvalidOperatorUsage(string input)
    {
        var parser = new StateMachineParser();
        var lexer = new Lexer();
        var tokens = lexer.Tokenize(input);
        Assert.Throws<ParserException>(() => parser.Parse(tokens));
    }

    private int CountNodes(INode node) => node switch
    {
        BinaryNode b => 1 + CountNodes(b.Left) + CountNodes(b.Right),
        FunctionNode f => 1 + f.Arguments.Sum(CountNodes),
        _ => 1
    };
}