using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;
using System.IO;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Bindings
{
    [Binding]
    public class ApplicationSettingsHook
    {
        [BeforeScenario(Order = 1)]
        public void InitialiseApplicationSettings(TestContext context)
        {
            context.ApplicationSettings = new ApplicationSettings
            {
                DbConnectionString = context.SqlDatabase.DatabaseInfo.ConnectionString,
                DistributedLockStorage = "UseDevelopmentStorage=true",
                AllowedHashstringCharacters = "46789BCDFGHJKLMNPRSTVWXY",
                Hashstring = "Test Hashstring",
                NServiceBusConnectionString = "UseLearningEndpoint=true",
                MinimumAgreementVersion = 4,
                UseLearningEndpointStorageDirectory = Path.Combine(context.TestDirectory.FullName, ".learningtransport")
            };
        }
    }
}
