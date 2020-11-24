using Dapper.Contrib.Extensions;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Bindings
{
    [Binding]
    [Scope(Tag = "activeCalendarPeriod")]
    public class ActiveCalendarPeriod
    {
        [BeforeScenario(Order = 20)]
        public async Task CreateDatabase(TestContext context)
        {
            await using var dbConnection = new SqlConnection(context.SqlDatabase.DatabaseInfo.ConnectionString);

            var calendar = await dbConnection.GetAllAsync<Data.ApprenticeshipIncentives.Models.CollectionPeriod>();
            var period = calendar.Single(x => x.CalendarYear == 2020 && x.CalendarMonth == 8);
            period.Active = true;

            await dbConnection.UpdateAsync(period);

            context.ActivePeriod = period;
        }

    }
}
