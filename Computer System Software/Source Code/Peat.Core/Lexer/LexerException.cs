namespace Peat.Core.Lexer;

public class LexerException(IEnumerable<string> errors) : Exception
{
    private IReadOnlyList<string> Errors { get; } = errors.ToList();

    public override string Message => string.Join("\n", Errors);
}