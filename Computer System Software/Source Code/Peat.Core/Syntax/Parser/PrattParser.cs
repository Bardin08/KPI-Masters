using Peat.Core.Lexer;
using Peat.Core.Syntax.Nodes;

namespace Peat.Core.Syntax.Parser;

public class PrattParser : IParser
{
    private readonly List<ParserError> _errors = new();
    private IEnumerator<Token> _tokenEnumerator = null!;
    private Token _current = null!;
    private int _parenthesesCount;

    private readonly Dictionary<string, (int Precedence, bool IsLeftAssociative)> _operators = new()
    {
        ["+"] = (1, true),
        ["-"] = (1, true),
        ["/"] = (2, true),
        ["*"] = (2, true)
    };

    public INode Parse(IEnumerable<Token> tokens)
    {
        var enumerable = tokens.ToList();
        if (enumerable.Count == 0)
            throw new ArgumentException("Tokens collection can't be empty", nameof(tokens));

        try
        {
            _tokenEnumerator = enumerable.GetEnumerator();
            _parenthesesCount = 0;
            Advance();

            _current = _tokenEnumerator.Current;
            var root = ParseExpression(0);

            if (_parenthesesCount != 0)
                _errors.Add(ParserError.UnmatchedOpeningParenthesis(_current));

            if (_errors.Count != 0)
                throw new BulkParserException(_errors);

            return root;
        }
        catch (Exception ex) when (ex is not BulkParserException)
        {
            throw new BulkParserException(_errors, ex);
        }
        finally
        {
            _tokenEnumerator.Dispose();
        }
    }

    private void Advance()
        => _current = _tokenEnumerator.MoveNext() ? _tokenEnumerator.Current : Token.Empty;

    private INode ParseExpression(int minPrecedence)
    {
        var left = ParsePrefix();

        while (true)
        {
            if (_current.TokenType is TokenType.Eof)
                break;

            if (_current.TokenType is TokenType.RParen)
            {
                if (_parenthesesCount <= 0)
                {
                    _parenthesesCount--;
                    _errors.Add(ParserError.UnmatchedClosingParenthesis(_current.Position));
                }

                break;
            }

            var (precedence, isLeftAssoc) = GetOperatorInfo(_current);
            if (precedence < minPrecedence)
                break;

            var op = _current.Value;
            Advance();

            var nextMinPrec = isLeftAssoc ? precedence + 1 : precedence;
            var right = ParseExpression(nextMinPrec);
            left = new BinaryNode(left, op, right);
        }

        return left;
    }

    private INode ParsePrefix()
    {
        var token = _current;
        Advance();

        return token.TokenType switch
        {
            TokenType.Number => new NumberNode(token.Value),
            TokenType.Variable => new VariableNode(token.Value),
            TokenType.Function => ParseFunction(token),
            TokenType.LParen => ParseParenthesis(),
            _ => HandleError($"Unexpected token: {token.Value}", token.Position)
        };
    }

    private INode ParseFunction(Token funcToken)
    {
        var args = new List<INode>();

        if (_current.TokenType != TokenType.LParen)
        {
            _errors.Add(ParserError.UnmatchedOpeningParenthesis(_current));
            return new FunctionNode(funcToken.Value, args);
        }

        _parenthesesCount++;
        Advance();

        if (_current.TokenType != TokenType.RParen)
        {
            do
            {
                args.Add(ParseExpression(0));

                if (_current.TokenType != TokenType.Comma)
                    break;

                Advance();
            } while (_current.TokenType != TokenType.RParen && _current.TokenType != TokenType.Eof);
        }

        if (_current.TokenType == TokenType.RParen)
        {
            _parenthesesCount--;
            Advance();
        }
        else
            _errors.Add(ParserError.UnclosedFunctionCall(funcToken));

        return new FunctionNode(funcToken.Value, args);
    }

    private INode ParseParenthesis()
    {
        _parenthesesCount++;
        var expr = ParseExpression(0);

        if (_current.TokenType != TokenType.RParen)
        {
            _errors.Add(ParserError.UnmatchedClosingParenthesis(_current.Position));
            return expr;
        }

        _parenthesesCount--;
        Advance();
        return expr;
    }

    private (int Precedence, bool IsLeftAssociative) GetOperatorInfo(Token token)
    {
        if (token.TokenType != TokenType.Operator || !_operators.TryGetValue(token.Value, out var info))
            return (0, true);

        return info;
    }

    private INode HandleError(string message, int position)
    {
        _errors.Add(new ParserError(message, null, position));
        return new ErrorNode(message);
    }
}