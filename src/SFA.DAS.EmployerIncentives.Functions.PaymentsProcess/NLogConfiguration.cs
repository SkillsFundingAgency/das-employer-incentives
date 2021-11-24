using System;
using System.IO;
using Microsoft.ApplicationInsights.NLogTarget;
using NLog;
using NLog.Common;
using NLog.Config;
using NLog.Targets;
using SFA.DAS.NLog.Targets.Redis.DotNetCore;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess
{
    public class NLogConfiguration
    {
        public void ConfigureNLog(string minimumLogLevel)
        {
            var appName = "das-employer-incentives-functions-payments-process";
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

            var eiFileTarget = new FileTarget("EI_Disk")
            {
                FileName = Path.Combine(Directory.GetCurrentDirectory(), $"logs\\{appName}.${{shortdate}}-EI.log"),
                Layout = fileTarget.Layout
            };
            config.AddTarget(eiFileTarget);

            var consoleTarget = new ColoredConsoleTarget("Console")
            {
                Layout = fileTarget.Layout
            };
            config.AddTarget(consoleTarget);

            var debuggerTarget = new DebuggerTarget("Debugger")
            {
                Layout = consoleTarget.Layout
            };
            config.AddTarget(debuggerTarget);

            config.AddRule(minimumLogLevel, LogLevel.Fatal, "Disk", "*");
            config.AddRule(minimumLogLevel, LogLevel.Fatal, "EI_Disk", "SFA.DAS.*");
            config.AddRule(minimumLogLevel, LogLevel.Fatal, "Console", "SFA.DAS.*");
            config.AddRule(minimumLogLevel, LogLevel.Fatal, "Debugger", "SFA.DAS.*");
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
