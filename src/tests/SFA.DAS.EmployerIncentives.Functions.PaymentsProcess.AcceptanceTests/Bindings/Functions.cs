using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Services;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Bindings
{
    [Binding]
    [Scope(Tag = "functions")]

    public class Functions
    {
        private readonly TestContext _context;

        public Functions(TestContext context)
        {
            _context = context;
        }

        [BeforeScenario(Order = 10)]
        public void InitialiseFunctions()
        {
            _context.PaymentsProcessFunctions = new TestPaymentsProcessFunctions(_context.SqlDatabase.DatabaseInfo.ConnectionString,
                _context.LearnerMatchApi.BaseAddress);
            _context.PaymentsProcessFunctions.Start();
        }
    }
}