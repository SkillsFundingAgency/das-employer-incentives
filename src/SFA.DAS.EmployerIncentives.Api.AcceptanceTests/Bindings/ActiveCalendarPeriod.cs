using Dapper.Contrib.Extensions;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Bindings
{
    [Binding]
    [Scope(Tag = "activeCalendarPeriod")]
    public class ActiveCalendarPeriod
    {
        /// <summary>
        /// Sets period 2020/1 (August) with Opening Date 2020-08-08 & Census Date 2020-08-31 as active
        /// </summary>
        [BeforeScenario(Order = 20)]
        public async Task SetActiveCollectionCalendarPeriod(AcceptanceTests.TestContext context)
        {
            await using var dbConnection = new SqlConnection(context.SqlDatabase.DatabaseInfo.ConnectionString);

            var calendar = await dbConnection.GetAllAsync<Data.ApprenticeshipIncentives.Models.CollectionCalendarPeriod>();
            var period = calendar.Single(x => x.CalendarYear == 2020 && x.CalendarMonth == 8);
            period.Active = true;
            period.PeriodEndInProgress = false;

            await dbConnection.UpdateAsync(period);

            context.ActivePeriod = period;
        }

    }
}
