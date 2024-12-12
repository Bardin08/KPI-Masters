namespace Peat.Core.Syntax.Parser;

public enum ParserState
{
    Start,
    Operand,
    Operator,
    LeftParen,
    Function,
    FunctionArgs,
    End
}