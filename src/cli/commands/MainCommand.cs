
using donetcore_cli.interfaces;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Threading.Tasks;

namespace donetcore_cli.commands
{
    [Command(Name = "dotnetdore_cli", OptionsComparison = System.StringComparison.InvariantCultureIgnoreCase)]
    [HelpOption("--help")]
    [VersionOptionFromMember("--version", MemberName = nameof(GetVersion))]
    [Subcommand(
        typeof(TestCommand)
        )]
    class MainCommand
    {
        private readonly ILogger<MainCommand> _logger = null;
        private IConsole _console = null;
        

        public MainCommand(ILoggerFactory loggerFactory, IConsole console)
        {
            _logger = _logger = loggerFactory.CreateLogger<MainCommand>();
            _console = console;
        }

        private Task<int> OnExecute(CommandLineApplication app)
        {
            app.ShowHelp();
            return Task.FromResult(0);
        }


        private static string GetVersion()
        {
            return typeof(MainCommand).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
        }
    }
}
