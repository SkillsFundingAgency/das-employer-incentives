using FluentAssertions;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Messages.Events;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "UpdateVrfCaseStatusForIncompleteCases")]
    public class UpdateVrfCaseStatusForIncompleteCasesSteps : StepsBase
    {
        private Account _testAccount;
        public UpdateVrfCaseStatusForIncompleteCasesSteps(TestContext testContext) : base(testContext)
        {
        }

        [Given(@"a legal entity has submitted vendor registration form details")]
        public async Task GivenAnApplicationHasBeenSubmitted()
        {
            _testAccount = TestContext.TestData.GetOrCreate<Account>();
            DataAccess.SetupAccount(_testAccount);
        }

        [When(@"an UpdateVrfCaseStatusForIncompleteCases job is requested")]
        public async Task WhenAnUpdateVrfCaseDetailsForNewApplicationsJobIsRequested()
        {
            await EmployerIncentiveApi.Put(
                $"/jobs",
                new JobRequest { Type = JobType.UpdateVrfCaseStatusForIncompleteCases, Data = null });

            EmployerIncentiveApi.Response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Then(@"the case statuses are requested for the legal entities")]
        public async Task ThenTheCaseStatusesAreRequestedForTheLegalEntities()
        {
            var publishedEvents = TestContext.TestData.GetOrCreate<List<UpdateLegalEntityVrfCaseStatusEvent>>();

            var publishedEvent = publishedEvents.SingleOrDefault(e => e.LegalEntityId == _testAccount.LegalEntityId);

            publishedEvent.Should().NotBeNull();
        }
    }
}
