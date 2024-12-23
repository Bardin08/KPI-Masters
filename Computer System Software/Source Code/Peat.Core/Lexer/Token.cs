namespace Peat.Core.Lexer;

public record Token(TokenType TokenType, int Position, string Value)
{
    public override string ToString() => $"[{TokenType}|{Position}: {Value}]";
}