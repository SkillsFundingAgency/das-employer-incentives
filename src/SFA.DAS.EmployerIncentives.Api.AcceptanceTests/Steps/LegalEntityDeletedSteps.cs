using System;
using System.Linq;
using FluentAssertions;
using SFA.DAS.EmployerIncentives.Data.Models;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using Newtonsoft.Json;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Queries.Account.GetApplications;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "LegalEntityDeleted")]
    public class LegalEntityDeletedSteps : StepsBase
    {
        private readonly TestContext _testContext;
        private readonly Account _testAccountTable;
        private HttpResponseMessage _response;
        private Fixture _fixture;

        public LegalEntityDeletedSteps(TestContext testContext) : base(testContext)
        {
            _testContext = testContext;
            _testAccountTable = _testContext.TestData.GetOrCreate<Account>();
            _fixture = new Fixture();
        }

        [When(@"a legal entity is removed from an account")]
        public async Task WhenALegalEntityIsRemovedFromAnAccount()
        {
            _response = await EmployerIncentiveApi.Delete($"/accounts/{_testAccountTable.Id}/legalEntities/{_testAccountTable.AccountLegalEntityId}");

            _response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }     

        [Given(@"a legal entity has submitted one or more applications")]
        public async Task GivenALegalEntityHasSubmittedOneOrMoreApplications()
        {
            var createApplicationRequest = _fixture.Create<CreateIncentiveApplicationRequest>();
            createApplicationRequest.AccountId = _testAccountTable.Id;
            createApplicationRequest.AccountLegalEntityId = _testAccountTable.AccountLegalEntityId;

            _response = await EmployerIncentiveApi.Post("/applications", createApplicationRequest);
            var applicationId = _response.Headers.Location.ToString().Substring("/applications/".Length);

            var submitApplicationRequest = _fixture.Create<SubmitIncentiveApplicationRequest>();
            submitApplicationRequest.AccountId = _testAccountTable.Id;
            submitApplicationRequest.IncentiveApplicationId = new Guid(applicationId);

            _response = await EmployerIncentiveApi.Patch($"/applications/{applicationId}", submitApplicationRequest);
        }

        [Then(@"the applications for that legal entity should be withdrawn")]
        public async Task ThenTheApplicationsForTheLegalEntityShouldBeWithdrawn()
        {
            _response = await EmployerIncentiveApi.Client.GetAsync($"/accounts/{_testAccountTable.Id}/legalentity/{_testAccountTable.AccountLegalEntityId}/applications");
            var json = await _response.Content.ReadAsStringAsync();
            var getApplicationsResponse = JsonConvert.DeserializeObject<GetApplicationsResponse>(json);
            getApplicationsResponse.ApprenticeApplications.Should().BeEmpty();
        }
    }
}
