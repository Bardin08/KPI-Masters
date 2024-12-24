using System.Diagnostics;
using Peat.Application.Expressions.Models;
using Peat.Core.Lexer;
using Peat.Core.Optimization;
using Peat.Core.Parallelization;
using Peat.Core.Syntax.Nodes;
using Peat.Core.Syntax.Parser;

namespace Peat.Application.Expressions.Services;

public class ExpressionService(ILexer lexer, IParser parser) : IExpressionService
{
    private readonly ILexer _lexer = lexer;
    private readonly IParser _parser = parser;

    public ValidationResult Validate(string expression)
    {
        var sw = Stopwatch.StartNew();
        var errors = new List<ExpressionError>();

        try
        {
            var tokens = _lexer.Tokenize(expression);
            _ = _parser.Parse(tokens);
        }
        catch (LexerException lex)
        {
            errors.AddRange(
                lex.Errors.Select(
                    x => new ExpressionError
                    {
                        Message = x.Message,
                        Type = ErrorType.Lexical,
                        Position = x.Position
                    }
                )
            );
        }
        catch (ParserException parse)
        {
            errors.Add(new ExpressionError
            {
                Message = parse.Message,
                Type = ErrorType.Syntax,
                Position = parse.Error.Token?.Position ?? parse.Error.Position
            });
        }

        return new ValidationResult
        {
            IsValid = !errors.Any(),
            Errors = errors,
            ValidationTime = sw.Elapsed
        };
    }

    public INode GetParallelTree(string expression)
    {
        var validationResult = Validate(expression);
        if (!validationResult.IsValid)
            return new ErrorNode("Invalid expression");

        var tokens = _lexer.Tokenize(expression);

        var expressionOptimizers = new List<ITokenOptimizer>
        {
            new UnaryOperatorsOptimizer(),
            new RedundantBracketsOptimizer(),
            new ConstantsOptimizer()
        };

        tokens = expressionOptimizers.Aggregate(tokens, (current, tokenOptimizer) => tokenOptimizer.Optimize(current));

        var parsedAst = new PrattParser().Parse(tokens);
        var parallelAst = new ParallelTreeBuilder().Build(parsedAst);

        return parallelAst.OriginalNode;
    }
}