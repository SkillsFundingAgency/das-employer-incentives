using FluentAssertions;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Data.Models;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using TechTalk.SpecFlow;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models;
using System.Data.SqlClient;
using System;
using SFA.DAS.EmployerIncentives.Enums;
using Dapper.Contrib.Extensions;
using System.Linq;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "EmploymentChecks")]
    public class EmploymentChecksSteps : StepsBase
    {
        private Account _account;
        private ApprenticeshipIncentive _apprenticeshipIncentive;
        private EmploymentCheck _employmentCheck;
        private Guid _correlationId;
        public EmploymentChecksSteps(TestContext testContext) : base(testContext)
        {
        }

        [Given(@"an apprenticeship incentive has submitted a new employment check")]
        public async Task GivenAnApprenticeshipIncentiveHasSubmittedANewEmploymentCheck()
        {
            _correlationId = Guid.NewGuid();
            _account = TestContext.TestData.GetOrCreate<Account>();
            _apprenticeshipIncentive = TestContext.TestData.GetOrCreate<ApprenticeshipIncentive>();
            _apprenticeshipIncentive.AccountId = _account.Id;
            _apprenticeshipIncentive.AccountLegalEntityId = _account.AccountLegalEntityId;
            _apprenticeshipIncentive.EmploymentChecks.Clear();

            _employmentCheck = Fixture.Build<EmploymentCheck>()
                .With(p => p.ApprenticeshipIncentiveId, _apprenticeshipIncentive.Id)
                .With(p => p.CorrelationId, _correlationId)
                .With(p => p.CreatedDateTime, DateTime.Today.AddDays(-10))
                .Without(p => p.Result)
                .Without(p => p.ResultDateTime)
                .Without(p => p.UpdatedDateTime)
                .Create();
            
            _apprenticeshipIncentive.EmploymentChecks.Add(_employmentCheck);

            await using var dbConnection = new SqlConnection(TestContext.SqlDatabase.DatabaseInfo.ConnectionString);
            await dbConnection.InsertAsync(_account, false);
            await dbConnection.InsertAsync(_apprenticeshipIncentive, false);
            await dbConnection.InsertAsync(_employmentCheck, false);
        }

        [When(@"the employment check result is returned with a result of (.*)")]
        public async Task WhenAddedLegalEntityEventIsTriggered(EmploymentCheckResultType checkResultType)
        {
            var response = await EmployerIncentiveApi.Put(
                    $"/employmentchecks/{_correlationId}",
                    new UpdateEmploymentCheckRequest
                    {
                        CorrelationId = _correlationId,
                        Result = checkResultType.ToString(),
                        DateChecked = DateTime.Today
                    });

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Then(@"the apprenticeship incentive employment check result is updated to (.*)")]
        public async Task ThenTheAprenticeshipIncentiveEmploymentCheckIsUpdatedTo(bool hasPassed)
        {
            await using var dbConnection = new SqlConnection(TestContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var employmentChecks = await dbConnection.GetAllAsync<EmploymentCheck>();

            employmentChecks.Count().Should().Be(1);

            var employmentCheck = employmentChecks.Single();
            employmentCheck.ApprenticeshipIncentiveId.Should().Be(_apprenticeshipIncentive.Id);
            employmentCheck.Result.Should().Be(hasPassed);
            employmentCheck.ResultDateTime.Should().Be(DateTime.Today);
            employmentCheck.UpdatedDateTime.Should().BeCloseTo(DateTime.Now, TimeSpan.FromMinutes(1));
        }
    }
}
