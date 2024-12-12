namespace Peat.Core.Syntax.Parser;

public class ParserException(ParserError error) : Exception(error.Message)
{
    public ParserError Error { get; } = error;
}

public class BulkParserException(List<ParserError> errors, Exception? innerException = null)
    : Exception(GetAggregatedError(errors), innerException)
{
    public List<ParserError> Errors { get; } = [];

    private static string GetAggregatedError(List<ParserError> errors)
        => string.Join(Environment.NewLine, errors.Select(e => e.Message));
}