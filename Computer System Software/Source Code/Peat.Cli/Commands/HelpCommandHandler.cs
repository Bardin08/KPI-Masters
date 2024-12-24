namespace Peat.Cli.Commands;

public record HelpCommandHandler : ICliCommandHandler
{
    public void Handle()
    {
        Console.WriteLine(
            """

            PEAT - Parallel Expression Analysis Tool

            Commands:
              analyze <expression>          Run full analysis pipeline
              tree <expression>             Visualize parallel execution tree

            Examples:
              peat analyze "a + b * (c - d)"
              peat tree "a + b * (c - d)"

            """);
    }
}