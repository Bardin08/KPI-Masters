using Peat.Core.Lexer;

namespace Peat.Core.Syntax.Parser;

public record ParserError(string Message, Token? Token, int Position = -1)
{
    public static ParserError UnmatchedClosingParenthesis(int pos) =>
        new("Unmatched closing parenthesis", null, pos);

    public static ParserError UnmatchedOpeningParenthesis(Token token) =>
        new("Unmatched opening parenthesis", token);

    public static ParserError UnexpectedToken(Token? token) =>
        new($"Unexpected token: {token?.TokenType ?? TokenType.Eof}", token);

    public static ParserError MissingRightOperand(Token token) =>
        new($"Missing right operand for operator '{token.Value}'", token);

    public static ParserError ConsecutiveOperators(Token token) =>
        new($"Consecutive operators are not allowed: '{token.Value}'", token);

    public static ParserError UnexpectedFunctionCall(Token token) =>
        new($"Unexpected function '{token.Value}' call", token);

    public static ParserError UnclosedFunctionCall(Token token) =>
        new($"Unclosed function call", token);

    public static ParserError FunctionRequiresAtLeastOneArgument(Token token) =>
        new($"Function '{token.Value}' requires at least one argument", token);

    public static ParserError InvalidTransition(
        ParserState currentState, TokenType currentTokenTokenType, int currentTokenPosition)
        => new(
            $"Invalid transition from {currentState.ToString()} to {currentTokenTokenType.ToString()} " +
            $"at position {currentTokenPosition}", null, currentTokenPosition);
}