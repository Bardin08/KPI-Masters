using Peat.Core.Lexer;
using Peat.Core.Syntax.Nodes;

namespace Peat.Core.Syntax.Parser;

public class Parser() : IParser
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

    private Token Expect(TokenType type)
    {
        var token = Consume();
        if (token?.TokenType != type)
            throw new ParserException($"Expected {type}, got {token?.TokenType ?? TokenType.Eof}");
        return token;
    }

    public INode Parse(IEnumerable<Token> tokens)
    {
        _tokens = new Queue<Token?>(tokens);
        return ParseExpression();
    }

    private INode ParseExpression(int precedence = 0)
    {
        var left = ParsePrimary();
        var parenCount = 0;

        while (Current?.TokenType == TokenType.Operator)
        {
            var op = Current;
            if (Precedences.GetPrecedence(op.Value) < precedence)
                break;

            ValidateOperator(op);
            Consume();

            if (Current == null || Current.TokenType == TokenType.Eof)
                throw new ParserException($"Missing right operand for operator '{op.Value}'");

            var right = ParseExpression(Precedences.GetPrecedence(op.Value) + 1);
            left = new BinaryNode(left, op.Value, right);
        }

        if (Current?.TokenType == TokenType.RParen && _parenCount == 0)
            throw new ParserException("Unmatched closing parenthesis");

        return left;
    }

    private void ValidateOperator(Token op)
    {
        if (_previous?.TokenType == TokenType.Operator)
            throw new ParserException($"Consecutive operators are not allowed: '{op.Value}'");
    }
    private INode ParsePrimary()
    {
        var token = Consume();
        switch (token.TokenType)
        {
            case TokenType.LParen:
                _parenCount++;
                var expr = ParseExpression();
                if (Current?.TokenType != TokenType.RParen)
                    throw new ParserException("Unmatched opening parenthesis");
                Consume();
                return expr;

            case TokenType.RParen:
                throw new ParserException("Unmatched closing parenthesis");

            case TokenType.Number:
                return new NumberNode(token.Value);

            case TokenType.Variable:
                return new VariableNode(token.Value);

            case TokenType.Function:
                return ParseFunction(token);

            default:
                throw new ParserException($"Unexpected token: {token.TokenType.ToString()}");
        }
    }

    private INode ParseFunction(Token? func)
    {
        var args = new List<INode>();
        Expect(TokenType.LParen);
    
        if (Current == null || Current.TokenType == TokenType.Eof)
            throw new ParserException("Unclosed function call");

        if (Current.TokenType == TokenType.RParen)
            throw new ParserException($"Function {func.Value} requires at least one argument");

        while (Current != null && Current.TokenType != TokenType.RParen)
        {
            if (Current.TokenType == TokenType.Comma)
                throw new ParserException("Unexpected comma in function arguments");
            
            args.Add(ParseExpression());
        
            if (Current == null || Current.TokenType == TokenType.Eof)
                throw new ParserException("Unclosed function call");
            
            if (Current.TokenType == TokenType.Comma)
            {
                Consume();
                if (Current?.TokenType == TokenType.RParen)
                    throw new ParserException("Trailing comma in function arguments");
            }
        }
    
        Expect(TokenType.RParen);
        return new FunctionNode(func.Value, args);
    }
}

public static class Precedences
{
    private static readonly Dictionary<string, int> Precedence = new()
    {
        // Parentheses
        {"(", 8},
        {")", 8},
       
        // Functions
        {"sin", 7},
        {"cos", 7},
        {"sqrt", 7},
       
        // Unary operators
        {"+u", 6},  // unary plus
        {"-u", 6},  // unary minus
       
        // Multiplicative
        {"*", 5},
        {"/", 5},
       
        // Additive 
        {"+", 4},
        {"-", 4},
       
        // Function argument separator
        {",", 1}
    };

    public static int GetPrecedence(string op) => Precedence.GetValueOrDefault(op, 0);

    public static bool IsLeftAssociative(string op) => op switch
    {
        "+" or "-" or "*" or "/" => true,
        _ => false
    };
}