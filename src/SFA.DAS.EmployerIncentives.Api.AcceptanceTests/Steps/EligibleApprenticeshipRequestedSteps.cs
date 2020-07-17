using FluentAssertions;
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

        public EligibleApprenticeshipRequestedSteps(TestContext testContext) : base(testContext)
        {
        }

        [Given(@"I am applying for the New Apprenticeship Incentive")]
        public void GivenIAmApplyingForTheNewApprenticeshipIncentive()
        {
        }

        [When(@"I request the eligibility of an apprenticeship")]
        public async Task WhenIRequestTheEligibilityOfAnApprenticeship()
        {
            var uln = 1234567;
            var url = $"eligible-apprenticeships/{uln}?startDate=2020-08-01&isApproved=true";
            _apiResult = await EmployerIncentiveApi.Client.GetAsync(url);
        }

        [Then(@"the status of the apprenticeship is returned")]
        public void ThenTheLegalEntitiesAreReturned()
        {
            _apiResult.StatusCode.Should().Be(HttpStatusCode.OK);
        }

    }
}
