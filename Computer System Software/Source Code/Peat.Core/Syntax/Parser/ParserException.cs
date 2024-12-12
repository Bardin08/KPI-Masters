namespace Peat.Core.Syntax.Parser;

public class ParserException(ParserError error) : Exception(error.Message)
{
    public ParserError Error { get; } = error;
}