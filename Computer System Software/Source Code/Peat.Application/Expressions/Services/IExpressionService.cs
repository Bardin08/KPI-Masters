using System.Diagnostics;
using Peat.Application.Expressions.Models;
using Peat.Core.Lexer;
using Peat.Core.Syntax.Parser;

namespace Peat.Application.Expressions.Services;

public interface IExpressionService
{
    Task<ValidationResult> ValidateAsync(string expression);
}

public class ExpressionService(ILexer lexer, IParser parser) : IExpressionService
{
    private readonly ILexer _lexer = lexer;
    private readonly IParser _parser = parser;

    public Task<ValidationResult> ValidateAsync(string expression)
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

        return Task.FromResult(new ValidationResult
        {
            IsValid = !errors.Any(),
            Errors = errors,
            ValidationTime = sw.Elapsed
        });
    }
}