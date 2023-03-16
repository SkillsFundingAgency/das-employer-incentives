using System;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace SFA.DAS.EmployerIncentives.Api
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddNLog(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            var env = Environment.GetEnvironmentVariable("EnvironmentName");
            var configFileName = "NLog.config";
            if (string.IsNullOrEmpty(env) || env.Equals("LOCAL", StringComparison.CurrentCultureIgnoreCase))
            {
                configFileName = "NLog.local.config";
            }
            var rootDirectory = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), ".."));
            var configFilePath = Directory.GetFiles(rootDirectory, configFileName, SearchOption.AllDirectories)[0];
            LogManager.Setup()
                .SetupExtensions(e => e.AutoLoadExtensions())
                .LoadConfigurationFromFile(configFilePath, optional: false)
                .LoadConfiguration(builder => builder.LogFactory.AutoShutdown = false)
                .GetCurrentClassLogger();

            serviceCollection.AddLogging((options) =>
            {
                options.AddFilter("SFA.DAS", LogLevel.Information); // this is because all logging is filtered out by default
                options.SetMinimumLevel(LogLevel.Trace);
                options.AddNLog(new NLogProviderOptions
                {
                    CaptureMessageTemplates = true,
                    CaptureMessageProperties = true
                });
#if DEBUG
                options.AddConsole();
#endif
            });

            return serviceCollection;
        }
    }
}
