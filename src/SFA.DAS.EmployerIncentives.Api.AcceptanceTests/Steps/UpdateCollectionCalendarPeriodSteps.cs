using Dapper;
using FluentAssertions;
using SFA.DAS.EmployerIncentives.Api.Types;
using System;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "UpdateCollectionCalendarPeriod")]
    public class UpdateCollectionCalendarPeriodSteps : StepsBase
    {
        public UpdateCollectionCalendarPeriodSteps(TestContext testContext) : base(testContext)
        {
        }

        [Given(@"there is an active collection period")]
        public async Task GivenThereIsAnActiveCollectionPeriod()
        {
            await SetupCollectionPeriod((short)DateTime.Now.Year, 1);
        }

        [When(@"the change of active collection period is requested")]
        public async Task WhenTheChangeOfActiveCollectionPeriodIsRequested()
        {
            var url = "collectionPeriods";
            var data = new UpdateCollectionPeriodRequest { PeriodNumber = 2, AcademicYear = 2021, Active = true };
            var apiResult = await EmployerIncentiveApi.Client.PatchAsync(url, data.GetStringContent());

            apiResult.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Then(@"the active collection period is changed")]
        public async Task ThenTheActiveCollectionPeriodIsChanged()
        {
            var newCollectionPeriodActive = await GetCollectionPeriodActive(2021, 2);
            newCollectionPeriodActive.Should().BeTrue();
            var oldCollectionPeriodActive = await GetCollectionPeriodActive(2021, 1);
            oldCollectionPeriodActive.Should().BeFalse();
        }

        private async Task SetupCollectionPeriod(short year, byte period)
        {
            using (var dbConnection = new SqlConnection(TestContext.SqlDatabase.DatabaseInfo.ConnectionString))
            {
                await dbConnection.ExecuteAsync($"update incentives.CollectionCalendar set Active = 0");

                await dbConnection.ExecuteAsync($"update incentives.CollectionCalendar set Active = 1 where PeriodNumber = @period and AcademicYear = @year", new { year, period });
            }
        }

        private async Task<bool> GetCollectionPeriodActive(short year, byte period)
        {
            using (var dbConnection = new SqlConnection(TestContext.SqlDatabase.DatabaseInfo.ConnectionString))
            {
                var sql = "select Active from incentives.CollectionCalendar where PeriodNumber = @period and AcademicYear = @year";

                var active = await dbConnection.QueryAsync<bool>(sql, new { year, period });

                return await Task.FromResult(active.FirstOrDefault());
            }
        }
    }
}
