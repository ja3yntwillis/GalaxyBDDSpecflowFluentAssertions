using System.Net;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using TestRunner.Utilities;

namespace TestRunner.UI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var configurationRunnerParser = new UIRunnerConfigurationParser();
            var configurationBaseParser = new ConfigurationBaseParser(configurationRunnerParser);

            configurationBaseParser.GetConfigurationArguments(args);
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseKestrel(options =>
                {
                    options.Listen(IPAddress.Any, RunnerConfiguration.DashboardPort);
                })
                .Build();
    }
}
