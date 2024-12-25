using Peat.Cli.Commands;

namespace Peat.Cli;

internal class CliCommandParser
{
    public ICliCommandHandler GetCommandHandler(string[] args)
    {
        if (args.Length == 0)
        {
            return new HelpCommandHandler();
        }

        var command = args[0].ToLower();
        var options = ParseOptions(args);

        return GetCommandHandler(command, args, options);
    }

    private static ICliCommandHandler GetCommandHandler(string command, string[] args, Dictionary<string, string> options)
    {
        return command switch
        {
            "analyze" => new AnalyzeExpressionCommandHandler(args, options),
            "tree" => new TreeCommandHandler(args, options),
            _ => throw new ArgumentException($"Unknown command: {command}")
        };
    }

    private static Dictionary<string, string> ParseOptions(string[] args)
    {
        var options = new Dictionary<string, string>();
        for (var i = 0; i < args.Length; i++)
        {
            if (!args[i].StartsWith('-'))
                continue;

            var key = args[i].TrimStart('-');
            var value = i + 1 < args.Length ? args[i + 1] : null;
            options[key] = value ?? "true";
            i++;
        }

        return options;
    }
}