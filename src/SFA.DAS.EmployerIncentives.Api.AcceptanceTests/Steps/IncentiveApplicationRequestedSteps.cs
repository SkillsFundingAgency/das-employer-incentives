using AutoFixture;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Data.Models;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using SFA.DAS.EmployerIncentives.DataTransferObjects.Queries;
using TechTalk.SpecFlow;
using IncentiveApplication = SFA.DAS.EmployerIncentives.DataTransferObjects.Queries.IncentiveApplication;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "IncentiveApplicationRequested")]
    public class IncentiveApplicationRequestedSteps : StepsBase
    {
        private readonly CreateIncentiveApplicationRequest _request;
        private IncentiveApplication _returnedApplication;
        private readonly Account _testAccountTable;

        public IncentiveApplicationRequestedSteps(TestContext testContext) : base(testContext)
        {
            _testAccountTable = testContext.TestData.GetOrCreate<Account>();
            var fixture = new Fixture();
            _request = fixture.Build<CreateIncentiveApplicationRequest>().With(x => x.AccountLegalEntityId, _testAccountTable.AccountLegalEntityId).Create();
        }

        [Given(@"An employer has selected the apprenticeships for their application")]
        public async Task GivenAnEmployerHasSelectedApprenticeships()
        {
            await DataAccess.SetupAccount(_testAccountTable);

            var url = $"applications";
            await EmployerIncentiveApi.Post(url, _request);
        }

        [When(@"They retrieve the application")]
        public async Task WhenTheyRetrieveTheApplication()
        {
            var url = $"/accounts/{_request.AccountId}/applications/{_request.IncentiveApplicationId}";
            var (_, data) =
                await EmployerIncentiveApi.Client.GetValueAsync<IncentiveApplication>(url);

            _returnedApplication = data;
        }

        [Then(@"the selected apprenticeships are returned")]
        public void ThenTheSavedApplicationIsReturned()
        {
            _returnedApplication.Should().NotBeNull();
            _returnedApplication.Apprenticeships.Count().Should().Be(_request.Apprenticeships.Count());
        }
    }
}
