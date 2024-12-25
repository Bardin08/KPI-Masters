using Peat.Cli;

var commandParser = new CliCommandParser();
var comamdHandler = commandParser.GetCommandHandler(args);

comamdHandler.Handle();
