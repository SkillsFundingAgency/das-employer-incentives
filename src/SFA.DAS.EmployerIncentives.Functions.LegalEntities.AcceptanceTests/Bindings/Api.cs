using System.Diagnostics;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Functions.LegalEntities.AcceptanceTests.Bindings
{
    [Binding]
    [Scope(Tag = "api")]
    public class Api
    {
        private const string AzureStorageEmulatorExe = @"C:\Program Files (x86)\Microsoft SDKs\Azure\Storage Emulator\AzureStorageEmulator.exe";
        private readonly TestContext _context;

        public Api(TestContext context)
        {
            _context = context;
        }

        [BeforeScenario()]
        public void InitialiseApi()
        {
            StartAzureStorageEmulator(); // TODO: Check with DevOps

            var webApi = new TestWebApi(_context);
            _context.ApiClient = webApi.CreateClient();
        }

        private static void StartAzureStorageEmulator()
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = AzureStorageEmulatorExe,
                    Arguments = "start",
                    WindowStyle = ProcessWindowStyle.Hidden
                }
            };

            process.Start();
        }
    }
}
