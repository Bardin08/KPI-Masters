namespace Peat.Core.Lexer;

public class LexerException(IEnumerable<LexerError> errors) : Exception
{
    public IReadOnlyList<LexerError> Errors { get; } = errors.ToList();

    public override string Message => string.Join("\n", Errors);
}