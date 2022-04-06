
using donetcore_cli.interfaces;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Threading.Tasks;

namespace donetcore_cli.commands
{
    [Command(
        Name = "test", 
        Description = "Test command with options", 
        OptionsComparison = System.StringComparison.InvariantCultureIgnoreCase
        )]
     
    [HelpOption("--help")]
    class TestCommand
    {
        private readonly ILogger<TestCommand> _logger = null;
        private IConsole _console = null;
        private readonly ITestService _testService = null;

        [Option(CommandOptionType.SingleValue, ShortName = "sc", LongName = "search_channel", Description = "Search Youtube Channel", ValueName = "Channel to search", ShowInHelpText = true)]
        public string ChannelName { get; set; } = null;

        public TestCommand(ILoggerFactory loggerFactory, IConsole console, ITestService testService)
        {
            _logger = loggerFactory.CreateLogger<TestCommand>();
            _console = console;
            _testService = testService;
        }

        private async Task<int> OnExecute(CommandLineApplication app)
        {
            if(ChannelName == null)
            {
                app.ShowHelp();
                _logger.LogInformation("search_channel is required");
                return 1;
            }
            await _testService.SearchVideo(ChannelName);
            return 0;
        }

    }
}
