namespace Peat.Core.Lexer;

public record Token(TokenType TokenType, int Position, string Value)
{
    public static readonly Token Empty = new(TokenType.Eof, -1, null!);

    public override string ToString() => $"[{TokenType}|{Position}: {Value}]";
}