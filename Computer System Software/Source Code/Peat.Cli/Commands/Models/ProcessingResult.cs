using Peat.Application.Expressions.Models;
using Peat.Cli.Commands.Models.Mappers;

namespace Peat.Cli.Commands.Models;

public record ProcessingResult
{
    public ValidationResult Validation { get; }
    public OptimizationResult Optimization { get; }
    public ParallelizationMetrics Parallelization { get; }

    public ProcessingResult(ProcessingContext context)
    {
        Validation = new ValidationResult(
            context.Validation.IsValid,
            context.Validation.Errors.Count,
            context.Validation.ValidationTime.TotalMilliseconds);

        Optimization = new OptimizationResult(
            context.Optimization.Steps.Count,
            context.Optimization.OptimizationTime.TotalMilliseconds,
            context.Optimization.Steps.Select(s => s.ToViewModel()).ToList());

        Parallelization = new ParallelizationMetrics(
            context.Parallelization.ParallelizationLevel,
            context.Parallelization.EstimatedProcessorCount,
            context.Parallelization.ParallelizationTime.TotalMilliseconds);
    }
}