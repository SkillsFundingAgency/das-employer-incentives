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
    [Scope(Feature = "ActivateCollectionCalendarPeriod")]
    public class ActiveCollectionCalendarPeriodSteps : StepsBase
    {
        public ActiveCollectionCalendarPeriodSteps(TestContext testContext) : base(testContext)
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
            var url = "collectionCalendar/period/activate";
            var data = new ActivateCollectionPeriodRequest { CollectionPeriodNumber = 2, CollectionPeriodYear = (short)DateTime.Now.Year };
            var apiResult = await EmployerIncentiveApi.Client.PatchAsync(url, data.GetStringContent());

            apiResult.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Then(@"the active collection period is changed")]
        public async Task ThenTheActiveCollectionPeriodIsChanged()
        {
            var newCollectionPeriodActive = await GetCollectionPeriodActive((short)DateTime.Now.Year, 2);
            newCollectionPeriodActive.Should().BeTrue();
            var oldCollectionPeriodActive = await GetCollectionPeriodActive((short)DateTime.Now.Year, 1);
            oldCollectionPeriodActive.Should().BeFalse();
        }

        private async Task SetupCollectionPeriod(short year, byte period)
        {
            using (var dbConnection = new SqlConnection(TestContext.SqlDatabase.DatabaseInfo.ConnectionString))
            {
                await dbConnection.ExecuteAsync($"update incentives.CollectionCalendar set Active = 0");

                await dbConnection.ExecuteAsync($"update incentives.CollectionCalendar set Active = 1 where PeriodNumber = @period and CalendarYear = @year", new { year, period });
            }
        }

        private async Task<bool> GetCollectionPeriodActive(short year, byte period)
        {
            using (var dbConnection = new SqlConnection(TestContext.SqlDatabase.DatabaseInfo.ConnectionString))
            {
                var sql = "select Active from incentives.CollectionCalendar where PeriodNumber = @period and CalendarYear = @year";

                var active = await dbConnection.QueryAsync<bool>(sql, new { year, period });

                return await Task.FromResult(active.FirstOrDefault());
            }
        }
    }
}
