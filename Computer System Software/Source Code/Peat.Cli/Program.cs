using Peat.Cli;
using Peat.Cli.Commands;

var commandParser = new CliCommandParser();
var comamdHandler = commandParser.GetCommandHandler(args);

comamdHandler.Handle();
