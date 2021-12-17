using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Bindings
{
    [Binding]
    public class ApplicationSettingsHook
    {
        [BeforeScenario(Order = 3)]
        public void InitialiseApplicationSettings(TestContext context)
        {
            context.ApplicationSettings = new ApplicationSettings
            {
                DbConnectionString = context.SqlDatabase.DatabaseInfo.ConnectionString,
                DistributedLockStorage = "UseDevelopmentStorage=true",
                AllowedHashstringCharacters = "46789BCDFGHJKLMNPRSTVWXY",
                Hashstring = "Test Hashstring",
                NServiceBusConnectionString = "UseLearningEndpoint=true",
                MinimumAgreementVersion = 4
            };
        }
    }
}
