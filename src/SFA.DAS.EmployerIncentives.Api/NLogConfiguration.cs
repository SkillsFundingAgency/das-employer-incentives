using Microsoft.ApplicationInsights.NLogTarget;
using NLog;
using NLog.Common;
using NLog.Config;
using NLog.Targets;
using SFA.DAS.NLog.Targets.Redis.DotNetCore;
using System;
using System.IO;

namespace SFA.DAS.EmployerIncentives.Api
{
    public class NLogConfiguration
    {
        public void ConfigureNLog(string minimumLogLevel)
        {
            const string appName = "das-employer-incentives-api";
            var env = Environment.GetEnvironmentVariable("EnvironmentName");
            var config = new LoggingConfiguration();

            if (string.IsNullOrEmpty(env) || env.Equals("LOCAL", StringComparison.CurrentCultureIgnoreCase))
            {
                AddLocalTarget(config, appName, LogLevel.FromString(minimumLogLevel));
            }
            else
            {
                AddRedisTarget(config, appName, LogLevel.FromString(minimumLogLevel));
                AddAppInsights(config, LogLevel.FromString(minimumLogLevel));
            }

            LogManager.Configuration = config;
        }

        private static void AddLocalTarget(LoggingConfiguration config, string appName, LogLevel minimumLogLevel)
        {
            InternalLogger.LogFile = Path.Combine(Directory.GetCurrentDirectory(), $"logs\\nlog-internal.{appName}.log");
            var fileTarget = new FileTarget("Disk")
            {
                FileName = Path.Combine(Directory.GetCurrentDirectory(), $"logs\\{appName}.${{shortdate}}.log"),
                Layout = "${longdate} [${uppercase:${level}}] [${logger}] - ${message} ${onexception:${exception:format=tostring}}"
            };
            config.AddTarget(fileTarget);

            config.AddRule(minimumLogLevel, LogLevel.Fatal, "Disk");
        }

        private static void AddRedisTarget(LoggingConfiguration config, string appName, LogLevel minimumLogLevel)
        {
            var target = new RedisTarget
            {
                Name = "RedisLog",
                AppName = appName,
                EnvironmentKeyName = "EnvironmentName",
                ConnectionStringName = "LoggingRedisConnectionString",
                IncludeAllProperties = true,
                Layout = "${message}"
            };

            config.AddTarget(target);
            config.AddRule(minimumLogLevel, LogLevel.Fatal, "RedisLog");
        }

        private static void AddAppInsights(LoggingConfiguration config, LogLevel minimumLogLevel)
        {
            var target = new ApplicationInsightsTarget
            {
                Name = "AppInsightsLog"
            };

            config.AddTarget(target);
            config.AddRule(minimumLogLevel, LogLevel.Fatal, "AppInsightsLog");
        }
    }
}
