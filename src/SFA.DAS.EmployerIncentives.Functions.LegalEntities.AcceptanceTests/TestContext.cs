using SFA.DAS.EmployerIncentives.Data.UnitTests.TestHelpers;
using SFA.DAS.HashingService;
using System;
using System.IO;

namespace SFA.DAS.EmployerIncentives.Functions.LegalEntities.AcceptanceTests
{
    public class TestContext
    {
        public DirectoryInfo TestDirectory { get; set; }
        public SqlDatabase SqlDatabase { get; set; }
        public EmployerIncentiveApi EmployerIncentiveApi { get; set; }
        public TestAccountApi AccountApi { get; set; }
        
        public TestData TestData { get; set; }

        public IHashingService HashingService { get; set; }        

        public TestContext()
        {
            TestDirectory = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), Guid.NewGuid().ToString()));
            if (!TestDirectory.Exists)
            {
                Directory.CreateDirectory(TestDirectory.FullName);
            }
            TestData = new TestData();
            HashingService = new HashingService.HashingService("46789BCDFGHJKLMNPRSTVWXY", "SFA: digital apprenticeship service");
        }
    }
}
