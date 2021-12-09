using System;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using NLog.Web;
using SFA.DAS.NServiceBus.Configuration.MicrosoftDependencyInjection;

namespace SFA.DAS.EmployerIncentives.Api
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var env = Environment.GetEnvironmentVariable("EnvironmentName");
            var configFileName = "nlog.config";
            if (string.IsNullOrEmpty(env) || env.Equals("LOCAL", StringComparison.CurrentCultureIgnoreCase))
            {
                configFileName = "nlog.local.config";
            }
            var rootDirectory = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), ".."));
            var configFilePath = Directory.GetFiles(rootDirectory, configFileName, SearchOption.AllDirectories)[0];
            NLogBuilder.ConfigureNLog(configFilePath).GetCurrentClassLogger();
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); })
                .UseNServiceBusContainer()
                .UseNLog();
    }
}
