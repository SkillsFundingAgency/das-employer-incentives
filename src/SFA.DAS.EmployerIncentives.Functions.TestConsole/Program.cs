using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;
using WireMock.Server;

namespace SFA.DAS.EmployerIncentives.Functions.TestConsole
#pragma warning disable 162
{
    public class Program
    {
        // Tweak these to cater for your needs 👇
        private const bool StartServiceBus = false;
        private const bool StartFakeLearnerMatchApi = true;
        private const bool StartFakeBusinessCentralFinanceApi = true;

        protected Program() { }

        private const string FunctionsBinFolder = "src\\SFA.DAS.EmployerIncentives.Functions.PaymentsProcess\\bin\\Debug\\netcoreapp3.1";

        static void Main(string[] args)
        {
            Run(args).Wait();
        }

        public static async Task Run(string[] args)
        {
            WireMockServer learnerApi = null;
            WireMockServer businessCentralApi = null;

            if (StartServiceBus)
            {
                await StartTestServiceBus(args);
            }

            if (StartFakeLearnerMatchApi)
            {
                var portNumber = GetMatchedLearnerApiPortNumberFromConfig();
                learnerApi = FakeLearnerMatchApiBuilder
                    .Create(portNumber)
                    .WithLearnerMatchingApi()
                    .Build();
            }

            if (StartFakeBusinessCentralFinanceApi)
            {
                var portNumber = GetBusinessCentralApiPortNumberFromConfig();
                businessCentralApi = FakeBusinessCentralApiBuilder
                    .Create(portNumber)
                    .WithPaymentRequestsEndpoint()
                    .Build();
            }

            Console.WriteLine("Press any key to stop the servers");
            Console.ReadKey();

            StopAndDispose(businessCentralApi);
            StopAndDispose(learnerApi);
        }

        private static void StopAndDispose(WireMockServer server)
        {
            if (server != null && server.IsStarted)
            {
                server.Stop();
            }
            server?.Dispose();
        }

        private static int GetMatchedLearnerApiPortNumberFromConfig()
        {
            var functionsConfigFile = Path.Combine(
                Directory.GetCurrentDirectory().Substring(0, Directory.GetCurrentDirectory().IndexOf("src", StringComparison.Ordinal)), FunctionsBinFolder,
                "local.settings.json");

            var settings = new MatchedLearnerApiSettings();
            new ConfigurationBuilder()
                .AddJsonFile(functionsConfigFile, optional: false) // Must have it!
                .AddEnvironmentVariables()
                .Build()
                .Bind("MatchedLearnerApi", settings);

            var port = int.Parse(settings.ApiBaseUrl.Split(":").Last());

            return port;
        }

        private static int GetBusinessCentralApiPortNumberFromConfig()
        {
            var functionsConfigFile = Path.Combine(
                Directory.GetCurrentDirectory().Substring(0, Directory.GetCurrentDirectory().IndexOf("src", StringComparison.Ordinal)), FunctionsBinFolder,
                "local.settings.json");

            var settings = new BusinessCentralApiClient();
            new ConfigurationBuilder()
                .AddJsonFile(functionsConfigFile, optional: false) // Must have it!
                .AddEnvironmentVariables()
                .Build()
                .Bind("BusinessCentralApi", settings);

            var port = int.Parse(settings.ApiBaseUrl.Split(":").Last());

            return port;
        }

        private static async Task StartTestServiceBus(string[] args)
        {
            var host = new HostBuilder()
                .UseEnvironment("local")
                .ConfigureHostConfiguration(configHost =>
                {
                    configHost.SetBasePath(Directory.GetCurrentDirectory());
                    configHost.AddJsonFile("host.json");
                    configHost.AddCommandLine(args);
                })
                .ConfigureAppConfiguration((hostContext, configApp) =>
                {
                    configApp.SetBasePath(Directory.GetCurrentDirectory());
                    configApp.AddJsonFile("local.settings.json", optional: true);
                    configApp.AddCommandLine(args);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddTransient<NServiceBusConsole>();
                    services.AddHostedService<LifetimeEventsHostedService>();
                })
                .ConfigureLogging((hostContext, configLogging) =>
                {
                    configLogging.AddConsole();
                    configLogging.AddDebug();
                })
                .UseConsoleLifetime()
                .Build();

            await host.RunAsync();
        }
    }

    public class MatchedLearnerApiSettings
    {
        public string ApiBaseUrl { get; set; }
        public string ClientId { get; set; }
        public string Version { get; set; }
    }
}
#pragma warning restore 162
