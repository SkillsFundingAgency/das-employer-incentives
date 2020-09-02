using AutoFixture;
using Dapper;
using FluentAssertions;
using Newtonsoft.Json;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Queries.NewApprenticeIncentive.GetApprenticeshipEligibility;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "EligibleApprenticeshipRequested")]
    public class EligibleApprenticeshipRequestedSteps : StepsBase
    {
        private HttpResponseMessage _apiResult;
        private long _uln;
        private long _accountId;
        private long _accountLegalEntityId;
        private IncentiveApplication _incentiveApplication;
        private IncentiveApplicationApprenticeship _incentiveApprenticeship;
        private Fixture _fixture;

        public EligibleApprenticeshipRequestedSteps(TestContext testContext) : base(testContext)
        {
            _fixture = new Fixture();
            _uln = _fixture.Create<long>();
            _accountId = _fixture.Create<long>();
            _accountLegalEntityId = _fixture.Create<long>();
            _incentiveApplication = _fixture.Build<IncentiveApplication>().Without(x => x.Apprenticeships).Create();
            _incentiveApprenticeship =
                _fixture.Build<IncentiveApplicationApprenticeship>()
                    .With(x => x.Uln, _uln)
                    .With(x => x.IncentiveApplicationId, _incentiveApplication.Id)
                    .Create();

            _incentiveApplication.Apprenticeships.Add(_incentiveApprenticeship);
        }

        [Given(@"I am applying for the New Apprenticeship Incentive")]
        public void GivenIAmApplyingForTheNewApprenticeshipIncentive()
        {

        }

        [Given(@"the ULN has been used on a previously submitted Incentive")]
        public Task GivenTheULNHasBeenUsedOnAPreviouslySubmittedIncentive()
        {
            return SetupApplicationAndApprenticeship("Submitted");
        }

        [Given(@"the ULN has been used on a draft Incentive Application")]
        public Task GivenTheULNHasBeenUsedOnAPreviouslyInProgressIncentive()
        {
            return SetupApplicationAndApprenticeship("InProgress");
        }


        [When(@"I request the eligibility of an apprenticeship that has a valid start date and ULN")]
        public async Task WhenIRequestTheEligibilityOfAnApprenticeship()
        {
            var url = $"eligible-apprenticeships/{_accountId}/{_accountLegalEntityId}";
            var newUln = _fixture.Create<long>();
            var data = new List<EligibleApprenticeshipCheckDetails>
            {
                new EligibleApprenticeshipCheckDetails { Uln = newUln, StartDate = new DateTime(2020,09,01), IsApproved = true }
            };

            _apiResult = await EmployerIncentiveApi.Client.PostValueAsync(url, data);
        }

        [When(@"I request the eligibility of an apprenticeship that has a valid start date and invalid ULN")]
        public async Task WhenIRequestTheEligibilityOfAnApprenticeshipThatIsAlreadyInUse()
        {
            var url = $"eligible-apprenticeships/{_accountId}/{_accountLegalEntityId}";
            var data = new List<EligibleApprenticeshipCheckDetails>
            {
                new EligibleApprenticeshipCheckDetails { Uln = _uln, StartDate = new DateTime(2020,10,01), IsApproved = true }
            };

            _apiResult = await EmployerIncentiveApi.Client.PostValueAsync(url, data);
        }


        [Then(@"the status of the apprenticeship is returned as eligible")]
        public async Task ThenTheStatusOfTheApprenticeshipIsReturnedAsEligible()
        {
            _apiResult.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseBody = await _apiResult.Content.ReadAsStringAsync();
            var response = JsonConvert.DeserializeObject<IEnumerable<EligibleApprenticeshipResult>>(responseBody);
            response.Count().Should().Be(1);
            response.First().Eligible.Should().Be(true);
        }

        [Then(@"the status of the apprenticeship is returned as not eligible")]
        public async Task ThenTheStatusOfTheApprenticeshipIsReturnedAsNotEligible()
        {
            _apiResult.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseBody = await _apiResult.Content.ReadAsStringAsync();
            var response = JsonConvert.DeserializeObject<IEnumerable<EligibleApprenticeshipResult>>(responseBody);
            response.Count().Should().Be(1);
            response.First().Eligible.Should().Be(false);
        }

        private async Task SetupApplicationAndApprenticeship(string status)
        {
            using (var dbConnection = new SqlConnection(TestContext.SqlDatabase.DatabaseInfo.ConnectionString))
            {
                await dbConnection.ExecuteAsync($"insert into IncentiveApplication(id, accountId, accountLegalEntityId, dateCreated, status, dateSubmitted, submittedByName, submittedByEmail) values " +
                                                $"(@id, @accountId, @accountLegalEntityId, @dateCreated, '{status}', @dateSubmitted, @submittedByName, @submittedByEmail)", _incentiveApplication);

                await dbConnection.ExecuteAsync($"insert into IncentiveApplicationApprenticeship(id, incentiveApplicationId, apprenticeshipId, firstName, lastName, dateOfBirth, " +
                                                "uln, plannedStartDate, apprenticeshipEmployerTypeOnApproval, TotalIncentiveAmount) values " +
                                                "(@id, @incentiveApplicationId, @apprenticeshipId, @firstName, @lastName, @dateOfBirth, " +
                                                "@uln, @plannedStartDate, @apprenticeshipEmployerTypeOnApproval, @totalIncentiveAmount)", _incentiveApprenticeship);
            }
        }
    }
}
