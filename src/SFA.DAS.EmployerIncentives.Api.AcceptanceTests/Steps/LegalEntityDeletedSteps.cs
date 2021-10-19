using AutoFixture;
using FluentAssertions;
using Newtonsoft.Json;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Queries.Account.GetApplications;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "LegalEntityDeleted")]
    public class LegalEntityDeletedSteps : StepsBase
    {
        private readonly Account _account;
        private HttpResponseMessage _response;

        public LegalEntityDeletedSteps(TestContext testContext) : base(testContext)
        {
            _account = TestContext.TestData.GetOrCreate<Account>();
        }

        [When(@"the legal entity is removed from an account")]
        public async Task WhenTheLegalEntityIsRemovedFromAnAccount()
        {
            _response = await EmployerIncentiveApi.Delete($"/accounts/{_account.Id}/legalEntities/{_account.AccountLegalEntityId}");

            _response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }     

        [Given(@"a legal entity has submitted one or more applications")]
        public async Task GivenALegalEntityHasSubmittedOneOrMoreApplications()
        {
            var createApplicationRequest = Fixture.Create<CreateIncentiveApplicationRequest>();
            createApplicationRequest.AccountId = _account.Id;
            createApplicationRequest.AccountLegalEntityId = _account.AccountLegalEntityId;

            _response = await EmployerIncentiveApi.Post("/applications", createApplicationRequest);
            var applicationId = _response.Headers.Location.ToString().Substring("/applications/".Length);

            var submitApplicationRequest = Fixture.Create<SubmitIncentiveApplicationRequest>();
            submitApplicationRequest.AccountId = _account.Id;
            submitApplicationRequest.IncentiveApplicationId = new Guid(applicationId);

            _response = await EmployerIncentiveApi.Patch($"/applications/{applicationId}", submitApplicationRequest);
        }

        [Then(@"the applications for that legal entity should be withdrawn")]
        public async Task ThenTheApplicationsForTheLegalEntityShouldBeWithdrawn()
        {
            _response = await EmployerIncentiveApi.Client.GetAsync($"/accounts/{_account.Id}/legalentity/{_account.AccountLegalEntityId}/applications");
            var json = await _response.Content.ReadAsStringAsync();
            var getApplicationsResponse = JsonConvert.DeserializeObject<GetApplicationsResponse>(json);
            getApplicationsResponse.ApprenticeApplications.Should().BeEmpty();
        }
    }
}
