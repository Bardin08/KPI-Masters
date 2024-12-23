using Peat.Core.Lexer;
using Peat.Core.Syntax.Nodes;

namespace Peat.Core.Syntax.Parser;

public class StateMachineParser : IParser
{
    private ParserState _currentState;
    private readonly Queue<Token> _tokens = new();
    private readonly Stack<Token> _operators = new();
    private readonly Stack<INode> _output = new();
    private readonly Stack<int> _argCounts = new();
    private int _parenCount;
    private Token? _currentToken;

    private static readonly Dictionary<ParserState, HashSet<TokenType>> ValidTransitions = new()
    {
        [ParserState.Start] =
        [
            TokenType.Number,
            TokenType.Variable,
            TokenType.Function,
            TokenType.LParen
        ],
        [ParserState.Operand] =
        [
            TokenType.Operator,
            TokenType.RParen,
            TokenType.Comma,
            TokenType.Eof
        ],
        [ParserState.Operator] =
        [
            TokenType.Number,
            TokenType.Variable,
            TokenType.Function,
            TokenType.LParen
        ],
        [ParserState.LeftParen] =
        [
            TokenType.Number,
            TokenType.Variable,
            TokenType.Function,
            TokenType.LParen
        ],
        [ParserState.Function] = [TokenType.LParen],
        [ParserState.FunctionArgs] =
        [
            TokenType.Number,
            TokenType.Variable,
            TokenType.Function,
            TokenType.LParen,
            TokenType.RParen,
            TokenType.Comma
        ]
    };

    public INode Parse(IEnumerable<Token> tokens)
    {
        Initialize(tokens);

        while (_currentState != ParserState.End)
        {
            _currentToken = GetNextToken();

            if (!IsValidTransition(_currentToken!.TokenType))
                throw new ParserException(
                    ParserError.InvalidTransition(_currentState, _currentToken.TokenType, _currentToken.Position));

            switch (_currentToken.TokenType)
            {
                case TokenType.Number:
                    HandleNumber();
                    break;
                case TokenType.Variable:
                    HandleVariable();
                    break;
                case TokenType.Operator:
                    HandleOperator();
                    break;
                case TokenType.Function:
                    HandleFunction();
                    break;
                case TokenType.LParen:
                    HandleLeftParen();
                    break;
                case TokenType.RParen:
                    HandleRightParen();
                    break;
                case TokenType.Comma:
                    HandleComma();
                    break;
                case TokenType.Eof:
                    HandleEof();
                    break;
            }
        }

        if (_parenCount != 0)
            throw new ParserException(
                ParserError.UnmatchedClosingParenthesis(_currentToken?.Position ?? 0));

        if (_output.Count != 1)
            throw new ParserException(
                ParserError.UnmatchedOpeningParenthesis(_currentToken!));

        return _output.Pop();
    }

    private void Initialize(IEnumerable<Token> tokens)
    {
        _tokens.Clear();
        _operators.Clear();
        _output.Clear();
        _argCounts.Clear();
        _parenCount = 0;
        _currentState = ParserState.Start;

        foreach (var token in tokens)
            _tokens.Enqueue(token);
    }

    private Token? GetNextToken()
    {
        return _tokens.Count > 0
            ? _tokens.Dequeue()
            : null;
    }

    private bool IsValidTransition(TokenType tokenType)
    {
        return ValidTransitions.ContainsKey(_currentState) &&
               ValidTransitions[_currentState].Contains(tokenType);
    }

    private void HandleNumber()
    {
        _output.Push(new NumberNode(_currentToken!.Value));
        _currentState = ParserState.Operand;
    }

    private void HandleVariable()
    {
        _output.Push(new VariableNode(_currentToken!.Value));
        _currentState = ParserState.Operand;
    }

    private void HandleOperator()
    {
        while (_operators.Count > 0)
        {
            var top = _operators.Peek();
            if (top.TokenType == TokenType.LParen)
                break;

            if (Precedences.GetPrecedence(top.Value) >= Precedences.GetPrecedence(_currentToken!.Value))
            {
                CreateOperatorNode(_operators.Pop());
            }
            else break;
        }

        _operators.Push(_currentToken!);
        _currentState = ParserState.Operator;
    }

    private void HandleFunction()
    {
        _operators.Push(_currentToken!);
        _argCounts.Push(0);
        _currentState = ParserState.Function;
    }

    private void HandleLeftParen()
    {
        _operators.Push(_currentToken!);
        _parenCount++;
        _currentState = ParserState.LeftParen;
    }

    private void HandleRightParen()
    {
        if (_parenCount <= 0)
            throw new ParserException(
                ParserError.UnmatchedClosingParenthesis(_currentToken!.Position));

        while (_operators.Count > 0)
        {
            var top = _operators.Peek();
            if (top.TokenType == TokenType.LParen)
                break;
            CreateOperatorNode(_operators.Pop());
        }

        if (_operators.Count == 0)
            throw new ParserException(
                ParserError.UnmatchedClosingParenthesis(_currentToken!.Position));

        _operators.Pop(); // Remove left paren
        _parenCount--;

        if (_operators.Count > 0 && _operators.Peek().TokenType == TokenType.Function)
        {
            var func = _operators.Pop();
            var args = new List<INode>();
            var argCount = _argCounts.Pop() + 1;

            for (int i = 0; i < argCount; i++)
                args.Insert(0, _output.Pop());

            _output.Push(new FunctionNode(func.Value, args));
        }

        _currentState = ParserState.Operand;
    }

    private void HandleComma()
    {
        while (_operators.Count > 0 && _operators.Peek().TokenType != TokenType.LParen)
            CreateOperatorNode(_operators.Pop());

        if (_argCounts.Count == 0)
            throw new ParserException(
                ParserError.UnexpectedToken(_currentToken!));

        _argCounts.Push(_argCounts.Pop() + 1);
        _currentState = ParserState.FunctionArgs;
    }

    private void HandleEof()
    {
        while (_operators.Count > 0)
        {
            var op = _operators.Pop();
            if (op.TokenType == TokenType.LParen)
            {
                if (_operators.Count > 0 && _operators.Peek().TokenType == TokenType.Function)
                    throw new ParserException(
                        ParserError.UnclosedFunctionCall(op));

                throw new ParserException(
                    ParserError.UnmatchedOpeningParenthesis(op));
            }

            CreateOperatorNode(op);
        }

        _currentState = ParserState.End;
    }

    private void CreateOperatorNode(Token op)
    {
        if (_output.Count < 2)
            throw new ParserException(
                ParserError.UnexpectedToken(op));

        var right = _output.Pop();
        var left = _output.Pop();
        _output.Push(new BinaryNode(left, op.Value, right));
    }
}