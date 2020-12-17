using AutoFixture;
using Dapper;
using FluentAssertions;
using SFA.DAS.EmployerIncentives.Data.Models;
using System.Data.SqlClient;
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
        private readonly long _uln;
        private readonly IncentiveApplication _incentiveApplication;
        private readonly IncentiveApplicationApprenticeship _incentiveApprenticeship;

        public EligibleApprenticeshipRequestedSteps(TestContext testContext) : base(testContext)
        {
            _uln = Fixture.Create<long>();
            _incentiveApplication = Fixture.Build<IncentiveApplication>().Without(x => x.Apprenticeships).Create();
            _incentiveApprenticeship =
                Fixture.Build<IncentiveApplicationApprenticeship>()
                    .With(x => x.ULN, _uln)
                    .With(x => x.IncentiveApplicationId, _incentiveApplication.Id)
                    .Create();

            _incentiveApplication.Apprenticeships.Add(_incentiveApprenticeship);
        }

        [Given(@"I am applying for the New Apprenticeship Incentive")]
        public void GivenIAmApplyingForTheNewApprenticeshipIncentive()
        {
            // intentionally blank
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


        [When(@"I request the eligibility of an apprenticeship")]
        public async Task WhenIRequestTheEligibilityOfAnApprenticeship()
        {
            var url = $"eligible-apprenticeships/{_uln}?startDate=2020-08-01&isApproved=true";
            _apiResult = await EmployerIncentiveApi.Client.GetAsync(url);
        }

        [Then(@"the status of the apprenticeship is returned as eligible")]
        public void ThenTheStatusOfTheApprenticeshipIsReturnedAsEligible()
        {
            _apiResult.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Then(@"the status of the apprenticeship is returned as not eligible")]
        public void ThenTheStatusOfTheApprenticeshipIsReturnedAsNotEligible()
        {
            _apiResult.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        private async Task SetupApplicationAndApprenticeship(string status)
        {
            using (var dbConnection = new SqlConnection(TestContext.SqlDatabase.DatabaseInfo.ConnectionString))
            {
                await dbConnection.ExecuteAsync($"insert into IncentiveApplication(id, accountId, accountLegalEntityId, dateCreated, status, dateSubmitted, submittedByName) values " +
                                                $"(@id, @accountId, @accountLegalEntityId, @dateCreated, '{status}', @dateSubmitted, @submittedByName)", _incentiveApplication);
                await dbConnection.ExecuteAsync($"insert into IncentiveApplicationApprenticeship(id, incentiveApplicationId, apprenticeshipId, firstName, lastName, dateOfBirth, " +
                                                "uln, plannedStartDate, apprenticeshipEmployerTypeOnApproval, TotalIncentiveAmount) values " +
                                                "(@id, @incentiveApplicationId, @apprenticeshipId, @firstName, @lastName, @dateOfBirth, " +
                                                "@uln, @plannedStartDate, @apprenticeshipEmployerTypeOnApproval, @totalIncentiveAmount)", _incentiveApprenticeship);
            }
        }
    }
}
