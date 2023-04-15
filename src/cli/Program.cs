using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using donetcore_cli.commands;
using donetcore_cli.interfaces;
using donetcore_cli.services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace donetcore_cli
{
    class Program
    {
        private static async Task<int> Main(string[] args)
        {

            // Configuration
            IConfigurationRoot Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(AppDomain.CurrentDomain.BaseDirectory + "\\appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            // Dependency Injection
            IHostBuilder builder = new HostBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddLogging(config =>
                    {
                        config.AddConsole();
                    });
                    services.AddHttpClient();

                    // Services Injection
                    _ = services.AddSingleton<ITestService>((serviceProvider) =>
                    {
                        return new TestService(
                            loggerFactory: serviceProvider.GetRequiredService<ILoggerFactory>(),
                            clientFactory: serviceProvider.GetRequiredService<IHttpClientFactory>()
                            );
                    });
                });

            // Run Application
            try
            {
                return await builder.RunCommandLineApplicationAsync<MainCommand>(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 1;
            }
        }
    }
}
