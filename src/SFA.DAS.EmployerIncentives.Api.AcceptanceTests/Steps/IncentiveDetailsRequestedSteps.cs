using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using SFA.DAS.EmployerIncentives.Queries.NewApprenticeIncentive.GetIncentiveDetails;
using SFA.DAS.EmployerIncentives.ValueObjects;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "IncentiveDetailsRequested")]
    public class IncentiveDetailsRequestedSteps : StepsBase
    {
        private GetIncentiveDetailsResponse _incentiveResponse;

        public IncentiveDetailsRequestedSteps(TestContext testContext) : base(testContext)
        {
        }

        [Given(@"an incentive exists")]
        public void GivenAnIncentiveExists()
        {
        }

        [When(@"a client requests the incentive details")]
        public async Task WhenAClientRequestsTheIncentiveDetails()
        {
            var url = $"/newapprenticeincentive";
            var (status, data) =
                await EmployerIncentiveApi.Client.GetValueAsync<GetIncentiveDetailsResponse>(url);

            status.Should().Be(HttpStatusCode.OK);

            _incentiveResponse = data;
        }

        [Then(@"the incentive details are returned")]
        public void ThenTheIncentiveDetailsAreReturned()
        {
            _incentiveResponse.Should().NotBeNull();
            _incentiveResponse.EligibilityStartDate.Should().Be(IncentiveProfiles.EligibilityStartDate);
            _incentiveResponse.EligibilityEndDate.Should().Be(IncentiveProfiles.EligibilityEndDate);
        }
    }
}
