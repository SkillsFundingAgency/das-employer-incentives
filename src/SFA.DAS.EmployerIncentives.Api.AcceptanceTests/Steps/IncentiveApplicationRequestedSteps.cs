using FluentAssertions;
using System.Threading.Tasks;
using AutoFixture;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Queries.NewApprenticeIncentive.GetApplication;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "IncentiveApplicationRequested")]
    public class IncentiveApplicationRequestedSteps : StepsBase
    {
        private readonly TestContext _testContext;
        private Fixture _fixture;
        private CreateIncentiveApplicationRequest _request;
        private GetApplicationResponse _getApplicationResponse;

        public IncentiveApplicationRequestedSteps(TestContext testContext) : base(testContext)
        {
            _testContext = testContext;
            _fixture = new Fixture();
            _request = _fixture.Create<CreateIncentiveApplicationRequest>();
        }

        [Given(@"An employer has selected the apprenticeships for their application")]
        public async Task GivenAnEmployerHasSelectedApprenticeships()
        {
            var url = $"applications";
            await EmployerIncentiveApi.Post(url, _request);
        }

        [When(@"They retrieve the application")]
        public async Task WhenTheyRetrieveTheApplication()
        {
            var url = $"/accounts/{_request.AccountId}/applications/{_request.IncentiveApplicationId}";
            var (status, data) =
                await EmployerIncentiveApi.Client.GetValueAsync<GetApplicationResponse>(url);

            _getApplicationResponse = data;
        }

        [Then(@"the selected apprenticeships are returned")]
        public void ThenTheSavedApplicationIsReturned()
        {
            _getApplicationResponse.Should().NotBeNull();
        }
    }
}
