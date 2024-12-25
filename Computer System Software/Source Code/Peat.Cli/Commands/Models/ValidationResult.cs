namespace Peat.Cli.Commands.Models;

public record ValidationResult(bool IsValid, int ErrorCount, double TimeMs);