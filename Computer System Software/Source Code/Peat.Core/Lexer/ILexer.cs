namespace Peat.Core.Lexer;

public interface ILexer
{
    IEnumerable<Token> Tokenize(string input);
    bool HasMoreTokens();
}