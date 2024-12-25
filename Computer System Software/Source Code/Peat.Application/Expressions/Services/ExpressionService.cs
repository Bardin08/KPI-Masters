using System.Diagnostics;
using Peat.Application.Expressions.Models;
using Peat.Application.Expressions.Models.Mappers;
using Peat.Core.Lexer;
using Peat.Core.Optimization;
using Peat.Core.Parallelization;
using Peat.Core.Syntax.Nodes;
using Peat.Core.Syntax.Parser;

namespace Peat.Application.Expressions.Services;

public class ExpressionService(
    ILexer lexer,
    IParser parser) : IExpressionService
{
    private readonly ILexer _lexer = lexer;
    private readonly IParser _parser = parser;

    private readonly IReadOnlyList<BaseOptimizer> _optimizers = (List<BaseOptimizer>)
    [
        new UnaryOperatorsOptimizer(),
        new RedundantBracketsOptimizer(),
        new ConstantsOptimizer()
    ];

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
        catch (BulkParserException pex)
        {
            var parserErrors = pex.Errors.Select(e =>
                new ExpressionError
                {
                    Message = e.Message,
                    Type = ErrorType.Syntax,
                    Position = e.Position
                });
            errors.AddRange(parserErrors);
        }

        return new ValidationResult
        {
            IsValid = !errors.Any(),
            Errors = errors,
            ValidationTime = sw.Elapsed
        };
    }


    public ParallelTreeResponse GetParallelTree(string expression)
    {
        var sw = Stopwatch.StartNew();
        var validationContext = Validate(expression);

        if (!validationContext.IsValid)
        {
            return new ParallelTreeResponse
            {
                Tree = new ErrorNode("Invalid expression"),
                Context = new ProcessingContext
                {
                    Validation = validationContext,
                    Optimization = new OptimizationContext
                    {
                        Steps = Array.Empty<OptimizationStep>(),
                        OptimizationTime = TimeSpan.Zero
                    },
                    Parallelization = new ParallelizationContext
                    {
                        ParallelizationLevel = 0,
                        EstimatedProcessorCount = 0,
                        ParallelizationTime = TimeSpan.Zero
                    }
                },
                ProcessingTime = sw.Elapsed
            };
        }

        var tokens = _lexer.Tokenize(expression);
        var optimizationContext = OptimizeTokens(tokens);
        var parallelizationContext = BuildParallelTree(optimizationContext.Steps.Last().OutputTokens);

        return new ParallelTreeResponse
        {
            Tree = parallelizationContext.Tree,
            Context = new ProcessingContext
            {
                Validation = validationContext,
                Optimization = optimizationContext,
                Parallelization = parallelizationContext.Context
            },
            ProcessingTime = sw.Elapsed
        };
    }

    private OptimizationContext OptimizeTokens(IEnumerable<Token> initialTokens)
    {
        var sw = Stopwatch.StartNew();
        var steps = new List<OptimizationStep>();
        var currentTokens = initialTokens.ToList();

        foreach (var optimizer in _optimizers)
        {
            var inputTokens = currentTokens.ToList();
            currentTokens = optimizer.Optimize(currentTokens).ToList();

            steps.Add(new OptimizationStep
            {
                OptimizerName = optimizer.GetType().Name,
                InputTokens = inputTokens,
                OutputTokens = currentTokens,
                Transformations = optimizer.TransformationContext.Steps
                    .Select(x => x.ToApplicationModel())
                    .ToList()
            });
        }

        return new OptimizationContext
        {
            Steps = steps,
            OptimizationTime = sw.Elapsed
        };
    }

    private (INode Tree, ParallelizationContext Context) BuildParallelTree(IEnumerable<Token> tokens)
    {
        var sw = Stopwatch.StartNew();
        var parsedAst = new PrattParser().Parse(tokens);
        var builder = new ParallelTreeBuilder();
        var parallelAst = builder.Build(parsedAst);

        var context = new ParallelizationContext
        {
            ParallelizationLevel = parallelAst.Level,
            EstimatedProcessorCount = EstimateProcessorCount(parallelAst),
            ParallelizationTime = sw.Elapsed
        };

        return (parallelAst.OriginalNode, context);
    }

    private int EstimateProcessorCount(ParallelNode node)
    {
        // TODO: Implementation to estimate optimal processor count
        return 1;
    }
}