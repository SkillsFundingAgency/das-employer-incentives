using FluentAssertions;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Enums;
using SFA.DAS.EmployerIncentives.Messages.Events;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "UpdateVrfCaseDetailsForNewApplications")]
    public class UpdateVrfCaseDetailsForNewApplicationsSteps : StepsBase
    {
        private Account _testAccount;
        private IncentiveApplication _testApplication;

        public UpdateVrfCaseDetailsForNewApplicationsSteps(TestContext testContext) : base(testContext)
        {
        }

        [Given(@"an application has been submitted")]
        public async Task GivenAnApplicationHasBeenSubmitted()
        {
            _testAccount = TestContext.TestData.GetOrCreate<Account>();
            _testAccount.VrfCaseId = null;
            _testApplication = TestContext.TestData.GetOrCreate<IncentiveApplication>();
            _testApplication.Status = IncentiveApplicationStatus.Submitted;
            _testApplication.AccountLegalEntityId = _testAccount.AccountLegalEntityId;

            DataAccess.SetupAccount(_testAccount);
            DataAccess.SetupApplication(_testApplication);
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
            var publishedEvents = TestContext.TestData.GetOrCreate<List<UpdateLegalEntityVrfCaseDetailsEvent>>();

            var publishedEvent = publishedEvents.SingleOrDefault(e => e.LegalEntityId == _testAccount.LegalEntityId);

            publishedEvent.Should().NotBeNull();
        }
    }
}
