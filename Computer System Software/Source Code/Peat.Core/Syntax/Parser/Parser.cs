using Peat.Core.Lexer;
using Peat.Core.Syntax.Nodes;

namespace Peat.Core.Syntax.Parser;

public class Parser : IParser
{
    private Queue<Token?> _tokens = [];
    private Token? Current => _tokens.TryPeek(out var token) ? token : null;
    private Token? _previous;
    private int _parenCount;


    private Token? Consume()
    {
        if (_tokens.Count is 0) return null;
        _previous = _tokens.Dequeue();
        return _previous;
    }

    private void Expect(TokenType type)
    {
        var token = Consume();
        if (token?.TokenType != type)
            throw new ParserException(ParserError.UnexpectedToken(token));
    }

    public INode Parse(IEnumerable<Token> tokens)
    {
        _tokens = new Queue<Token?>(tokens);
        return ParseExpression();
    }

    private INode ParseExpression(int precedence = 0)
    {
        var left = ParsePrimary();

        while (Current?.TokenType == TokenType.Operator)
        {
            var op = Current;
            if (Precedences.GetPrecedence(op.Value) < precedence)
                break;

            ValidateOperator(op);
            Consume();

            if (Current == null || Current.TokenType == TokenType.Eof)
                throw new ParserException(ParserError.MissingRightOperand(op));

            var right = ParseExpression(Precedences.GetPrecedence(op.Value) + 1);
            left = new BinaryNode(left, op.Value, right);
        }

        if (Current?.TokenType == TokenType.RParen && _parenCount == 0)
            throw new ParserException(ParserError.UnmatchedClosingParenthesis(_previous?.Position + 1 ?? -1));

        return left;
    }

    private void ValidateOperator(Token op)
    {
        if (_previous?.TokenType == TokenType.Operator)
            throw new ParserException(ParserError.ConsecutiveOperators(op));
    }
    private INode ParsePrimary()
    {
        var token = Consume();
        switch (token!.TokenType)
        {
            case TokenType.LParen:
                _parenCount++;
                var expr = ParseExpression();
                if (Current?.TokenType != TokenType.RParen)
                    throw new ParserException(ParserError.UnmatchedOpeningParenthesis(token));
                Consume();
                _parenCount--;
                return expr;

            case TokenType.RParen:
                throw new ParserException(ParserError.UnmatchedClosingParenthesis(token.Position));

            case TokenType.Number:
                return new NumberNode(token.Value);

            case TokenType.Variable:
                return new VariableNode(token.Value);

            case TokenType.Function:
                return ParseFunction(token);

            case TokenType.Operator:
            case TokenType.Invalid:
            case TokenType.Eof:
            case TokenType.Comma:
            default:
                throw new ParserException(ParserError.UnexpectedToken(token));
        }
    }

    private INode ParseFunction(Token? func)
    {
        var args = new List<INode>();
        Expect(TokenType.LParen);
        _parenCount++;
    
        if (Current == null || Current.TokenType == TokenType.Eof)
            throw new ParserException(ParserError.UnexpectedFunctionCall(func!));

        if (Current.TokenType == TokenType.RParen)
            throw new ParserException(ParserError.FunctionRequiresAtLeastOneArgument(func!));

        while (Current != null && Current.TokenType != TokenType.RParen)
        {
            if (Current.TokenType == TokenType.Comma)
                throw new ParserException(ParserError.UnexpectedToken(Current));
            
            args.Add(ParseExpression());

            if (Current == null || Current.TokenType == TokenType.Eof)
                throw new ParserException(ParserError.UnclosedFunctionCall(func!));
            
            if (Current.TokenType == TokenType.Comma)
            {
                Consume();
                if (Current?.TokenType == TokenType.RParen)
                    throw new ParserException(ParserError.UnexpectedToken(Current));
            }
        }
    
        Expect(TokenType.RParen);
        _parenCount--;

        return new FunctionNode(func!.Value, args);
    }
}