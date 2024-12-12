namespace Peat.Core.Lexer;

public class Lexer : ILexer
{
    private int _position = 0;

    public IEnumerable<Token> Tokenize(string input)
    {
        var tokens = new List<Token>();
        var errors = new List<LexerError>();

        var span = input.AsSpan();
        _position = 0;

        while (true)
        {
            var token = NextToken(span);
        
            if (token.TokenType == TokenType.Invalid)
            {
                errors.Add(
                    new LexerError($"Invalid token '{token.Value}' at position {token.Position}", token.Position));
            }
        
            tokens.Add(token);
        
            if (token.TokenType is TokenType.Eof)
                break;
        }

        if (errors.Any())
            throw new LexerException(errors);

        return tokens;
    }

    private Token NextToken(ReadOnlySpan<char> span)
    {
        while (_position < span.Length)
        {
            if (char.IsWhiteSpace(span[_position]))
            {
                while (_position < span.Length && char.IsWhiteSpace(span[_position]))
                    _position++;
                continue;
            }

            var currentPosition = _position;
            var current = span[currentPosition];
            _position++;

            switch (current)
            {
                case '(':
                    return new Token(TokenType.LParen, currentPosition, "(");
                case ')':
                    return new Token(TokenType.RParen, currentPosition, ")");
                case ',':
                    return new Token(TokenType.Comma, currentPosition, ",");
                case '+':
                case '-':
                case '*':
                case '/':
                    return new Token(TokenType.Operator, currentPosition, current.ToString());
            }

            if (char.IsLetter(current))
            {
                _position--; // Move back to start of identifier
                var identifier = ReadIdentifier(span);
                var tokenType = IsFunction(identifier) ? TokenType.Function : TokenType.Variable;
                return new Token(tokenType, currentPosition, identifier.ToString());
            }

            if (char.IsDigit(current))
            {
                _position--; // Move back to start of number
                var number = ReadNumber(span);
                return new Token(TokenType.Number, currentPosition, number.ToString());
            }

            return new Token(TokenType.Invalid, currentPosition, current.ToString());
        }

        return new Token(TokenType.Eof, _position, "EOF");
    }

    private ReadOnlySpan<char> ReadIdentifier(ReadOnlySpan<char> span)
    {
        var start = _position;
        while (_position < span.Length && (char.IsLetterOrDigit(span[_position]) || span[_position] == '_'))
        {
            _position++;
        }

        return span[start.._position];
    }

    private ReadOnlySpan<char> ReadNumber(ReadOnlySpan<char> span)
    {
        var start = _position;
        bool hasDecimalPoint = false;

        while (_position < span.Length)
        {
            if (span[_position] == '.' && !hasDecimalPoint)
            {
                hasDecimalPoint = true;
                _position++;
            }
            else if (char.IsDigit(span[_position]))
            {
                _position++;
            }
            else
            {
                break;
            }
        }

        return span[start.._position];
    }

    private bool IsFunction(ReadOnlySpan<char> identifier)
    {
        return identifier.SequenceEqual("sin".AsSpan()) ||
               identifier.SequenceEqual("cos".AsSpan()) ||
               identifier.SequenceEqual("sqrt".AsSpan());
    }
}