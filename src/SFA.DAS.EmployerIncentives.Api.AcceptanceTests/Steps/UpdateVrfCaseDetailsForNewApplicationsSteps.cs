using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Enums;
using SFA.DAS.EmployerIncentives.Messages.Events;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "UpdateVrfCaseDetailsForNewApplications")]
    public class UpdateVrfCaseDetailsForNewApplicationsSteps : StepsBase
    {
        private readonly TestContext _testContext;
        private readonly IServiceProvider _serviceProvider;
        private Account _testAccount;
        private IncentiveApplication _testApplication;

        public UpdateVrfCaseDetailsForNewApplicationsSteps(TestContext testContext) : base(testContext)
        {
            _testContext = testContext;
            _testAccount = testContext.TestData.GetOrCreate<Account>();
            _testAccount.VrfCaseId = null;
            _testApplication = testContext.TestData.GetOrCreate<IncentiveApplication>();
            _testApplication.Status = IncentiveApplicationStatus.Submitted;
            _testApplication.AccountLegalEntityId = _testAccount.AccountLegalEntityId;
        }

        [Given(@"an application has been submitted")]
        public async Task GivenAnApplicationHasBeenSubmitted()
        {
            var dbContext = _testContext.GetApiService<EmployerIncentivesDbContext>();
            dbContext.Accounts.Add(_testAccount);
            dbContext.Applications.Add(_testApplication);
            await dbContext.SaveChangesAsync();
        }

        [When(@"an UpdateVrfCaseDetailsForNewApplications job is requested")]
        public async Task WhenAnUpdateVrfCaseDetailsForNewApplicationsJobIsRequested()
        {
            await EmployerIncentiveApi.Put(
                $"/jobs",
                new JobRequest { Type = JobType.UpdateVrfCaseDetailsForNewApplications, Data = null });

            EmployerIncentiveApi.Response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Then(@"the case details are requested for the legal entity")]
        public async Task ThenTheLegalEntitiesShouldBeAvailableInEmployerIncentives()
        {
            var publishedEvents = _testContext.TestData.GetOrCreate<List<GetLegalEntityVrfCaseDetailsEvent>>();

            var publishedEvent = publishedEvents.SingleOrDefault(e => e.LegalEntityId == _testAccount.LegalEntityId);

            publishedEvent.Should().NotBeNull();
        }
    }
}
