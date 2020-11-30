using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Web;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Services
{
    public class TestPaymentsProcessFunctionsConfigurator
    {
        private const string TestConfigFile = "local.settings.json";
        public TestSettings Settings { get; } = new TestSettings();
        private readonly string _databaseConnectionString;
        private readonly string _learnerMatchApiBaseUrl;
        private readonly string _paymentsApiBaseUrl;

        public TestPaymentsProcessFunctionsConfigurator(string databaseConnectionString, string learnerMatchApiBaseUrl, string paymentsApiBaseUrl)
        {
            _databaseConnectionString = databaseConnectionString;
            _learnerMatchApiBaseUrl = learnerMatchApiBaseUrl;
            _paymentsApiBaseUrl = paymentsApiBaseUrl;
        }

        public TestPaymentsProcessFunctionsConfigurator Setup()
        {
            ReadSettings();
            Settings.FunctionApplicationPath = GetFunctionAppDirectory();
            Settings.FunctionHostPath = GetFunctionsCoreToolsLocation();
            var functionConfig = ReplaceFunctionsConfig();
            ReplaceDbConnectionString(functionConfig);
            ReplaceLearnerMatchUrlString(functionConfig);
            ReplacePaymentsUrlString(functionConfig);

            return this;
        }

        private string ReplaceFunctionsConfig()
        {
            var functionConfig = Path.Combine(Settings.FunctionApplicationPath, TestConfigFile);
            File.Copy(TestConfigFile, functionConfig, overwrite: true);
            return functionConfig;
        }

        private void ReadSettings()
        {
            new ConfigurationBuilder()
                .AddJsonFile(TestConfigFile, optional: false) // Must have it!
                .AddEnvironmentVariables()
                .Build()
                .Bind("TestSettings", Settings);
        }

        private string GetFunctionAppDirectory()
        {
            var dir = Path.Combine(Directory.GetCurrentDirectory()
                    .Substring(0, Directory.GetCurrentDirectory().IndexOf("src", StringComparison.Ordinal)),
                Settings.FunctionApplicationPath);
            if (!Directory.Exists(dir)) throw new Exception($"Wrong path to functions' bin folder. Check '{TestConfigFile}' of the test project");
            return dir;
        }

        private string GetFunctionsCoreToolsLocation()
        {
            var dir = Environment.ExpandEnvironmentVariables(Settings.FunctionHostPath);
            if (!File.Exists(dir)) throw new Exception($"Wrong path to Azure Functions Core tools. Check '{TestConfigFile}' of the test project");
            return dir;
        }

        private void ReplaceDbConnectionString(string pathToConfig)
        {
            var escapedConnString = HttpUtility.JavaScriptStringEncode(_databaseConnectionString);
            File.WriteAllText(pathToConfig, (File.ReadAllText(pathToConfig)).Replace("DB_CONNECTION_STRING", escapedConnString));
        }

        private void ReplaceLearnerMatchUrlString(string pathToConfig)
        {
            var baseAddress = HttpUtility.JavaScriptStringEncode(_learnerMatchApiBaseUrl);
            File.WriteAllText(pathToConfig, (File.ReadAllText(pathToConfig)).Replace("LEARNER_MATCH_API_URL", baseAddress));
        }

        private void ReplacePaymentsUrlString(string pathToConfig)
        {
            var baseAddress = HttpUtility.JavaScriptStringEncode(_paymentsApiBaseUrl);
            File.WriteAllText(pathToConfig, (File.ReadAllText(pathToConfig)).Replace("PAYMENTS_API_URL", baseAddress));
        }
    }
}