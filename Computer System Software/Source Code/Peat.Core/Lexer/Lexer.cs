namespace Peat.Core.Lexer;

public ref struct Lexer() : ILexer
{
    private ReadOnlySpan<char> _input;
    private int _position = 0;
    private char CurrentChar => _position < _input.Length ? _input[_position] : '\0';

    public IEnumerable<Token> Tokenize(string input)
    {
        var tokens = new List<Token>();
        var errors = new List<string>();

        _input = input.AsSpan();
        _position = 0;

        while (true)
        {
            var token = NextToken();
        
            if (token.TokenType == TokenType.Invalid)
            {
                errors.Add($"Invalid token '{token.Value}' at position {token.Position}");
            }
        
            tokens.Add(token);
        
            if (token.TokenType is TokenType.Eof)
                break;
        }

        if (errors.Any())
            throw new LexerException(errors);

        return tokens;
    }

    private Token NextToken()
    {
        while (_position < _input.Length)
        {
            if (char.IsWhiteSpace(CurrentChar))
            {
                while (_position < _input.Length && char.IsWhiteSpace(_input[_position]))
                    _position++;
                continue;
            }

            var currentPosition = _position;
            var current = CurrentChar;
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
                var identifier = ReadIdentifier();
                var tokenType = IsFunction(identifier) ? TokenType.Function : TokenType.Variable;
                return new Token(tokenType, currentPosition, identifier.ToString());
            }

            if (char.IsDigit(current))
            {
                _position--; // Move back to start of number
                var number = ReadNumber();
                return new Token(TokenType.Number, currentPosition, number.ToString());
            }

            return new Token(TokenType.Invalid, currentPosition, current.ToString());
        }

        return new Token(TokenType.Eof, _position, "EOF");
    }

    private ReadOnlySpan<char> ReadIdentifier()
    {
        var start = _position;
        while (_position < _input.Length && (char.IsLetterOrDigit(_input[_position]) || _input[_position] == '_'))
        {
            _position++;
        }

        return _input[start.._position];
    }

    private ReadOnlySpan<char> ReadNumber()
    {
        var start = _position;
        bool hasDecimalPoint = false;

        while (_position < _input.Length)
        {
            if (_input[_position] == '.' && !hasDecimalPoint)
            {
                hasDecimalPoint = true;
                _position++;
            }
            else if (char.IsDigit(_input[_position]))
            {
                _position++;
            }
            else
            {
                break;
            }
        }

        return _input[start.._position];
    }

    private bool IsFunction(ReadOnlySpan<char> identifier)
    {
        return identifier.SequenceEqual("sin".AsSpan()) ||
               identifier.SequenceEqual("cos".AsSpan()) ||
               identifier.SequenceEqual("sqrt".AsSpan());
    }

    public bool HasMoreTokens() => _position < _input.Length;
}