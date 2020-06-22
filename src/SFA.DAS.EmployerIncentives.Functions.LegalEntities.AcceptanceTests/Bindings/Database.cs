using TechTalk.SpecFlow;
using SFA.DAS.EmployerIncentives.Data.UnitTests.TestHelpers;

namespace SFA.DAS.EmployerIncentives.Functions.LegalEntities.AcceptanceTests.Bindings
{
    [Binding]
    [Scope(Tag = "database")]
    public class Database
    {
        private readonly TestContext _context;

        public Database(TestContext context)
        {
            _context = context;
        }

        [BeforeScenario()]
        public void InitialiseDatabase()
        {
            _context.DatabaseProperties = SqlHelper.CreateTestDatabase(_context.TestDirectory);
        }
    }
}
