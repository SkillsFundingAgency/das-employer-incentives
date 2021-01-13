using FluentAssertions;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Commands.Services.AccountApi;
using SFA.DAS.EmployerIncentives.Messages.Events;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using WireMock.Matchers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "LegalEntitiesDataLoad")]
    public class LegalEntitiesDataLoadSteps : StepsBase
    {
        private readonly TestContext _testContext;
        private readonly HttpStatusCode _expectedResult = HttpStatusCode.NoContent;

        public LegalEntitiesDataLoadSteps(TestContext testContext) : base(testContext)
        {
            _testContext = testContext;
        }

        [Given(@"legal entities exist in Managed Apprenticeships")]
        public void GivenLegalEntitiesExistInMa()
        {
            var totalPages = 3;

            for (int pageNumber = 1; pageNumber <= totalPages; pageNumber++)
            {
                SetUpMockPageResponse(pageNumber, totalPages);
            }
        }        

        [When(@"a RefreshLegalEntities job is requested")]
        public async Task WhenArefreshLegalEntitiesJobisrequested()
        {
            var data = new Dictionary<string, object>
            {
                { "PageNumber", 1 },
                { "PageSize", 200 }
            };

            await EmployerIncentiveApi.Put(
                    $"/jobs",
                   new JobRequest { Type = JobType.RefreshLegalEntities, Data = data });

            EmployerIncentiveApi.GetLastResponse().StatusCode.Should().Be(_expectedResult);
        }

        [Then(@"the legal entities are available in employer incentives")]
        public void ThenTheLegalEntitiesShouldBeAvailableInEmployerIncentives()
        {
            var totalPages = 3;

            for (int pageNumber = 1; pageNumber <= totalPages; pageNumber++)
            {
                var testData = _testContext.TestData.GetOrCreate<PagedModel<AccountLegalEntity>>($"PagedModel<AccountLegalEntity>_{pageNumber}");
                testData.Data.Count.Should().Be(3);

                if (IsFirstPage(pageNumber))
                {
                    AssertAccountServiceWasCalledForPageNumber(pageNumber);

                    foreach (AccountLegalEntity legalEntity in testData.Data)
                    {
                        AssertRefreshLegalEntityWasPublished(legalEntity);
                    }
                }
                else
                {
                    AssertRefreshLegalEntitiesEventsWerePublishedForPage(pageNumber);
                }
            }
        }

        private bool IsFirstPage(int pageNumber)
        {
            return pageNumber == 1;
        }

        private void AssertAccountServiceWasCalledForPageNumber(int pageNumber)
        {
            var requests = _testContext
                       .AccountApi
                       .MockServer
                       .FindLogEntries(
                           Request
                           .Create()
                           .WithPath(u => u.StartsWith($"/api/accountlegalentities"))
                           .WithParam("pageNumber", true, new[] { $"{pageNumber}" })
                           .UsingGet());

            requests.AsEnumerable().Count().Should().Be(1);
        }

        private void AssertRefreshLegalEntityWasPublished(AccountLegalEntity accountLegalEntity)
        {
            var publishedEvents = _testContext.EventsPublished.OfType<RefreshLegalEntityEvent>();

            var publishedEvent = publishedEvents.SingleOrDefault(e => e.AccountId == accountLegalEntity.AccountId &&
                                             e.AccountLegalEntityId == accountLegalEntity.AccountLegalEntityId &&
                                             e.LegalEntityId == accountLegalEntity.LegalEntityId &&
                                             e.OrganisationName == accountLegalEntity.Name);

            publishedEvent.Should().NotBeNull();            
        }

        private void AssertRefreshLegalEntitiesEventsWerePublishedForPage(int pageNumber)
        {
            var publishedEvents = _testContext.EventsPublished.OfType<RefreshLegalEntitiesEvent>();
            var publishedEvent = publishedEvents.SingleOrDefault(e => e.PageNumber == pageNumber);
            publishedEvent.Should().NotBeNull();
        }

        private void SetUpMockPageResponse(int pageNumber, int totalPages)
        {
            var testData = _testContext.TestData.GetOrCreate<PagedModel<AccountLegalEntity>>($"PagedModel<AccountLegalEntity>_{pageNumber}");
            testData.TotalPages = totalPages;
            testData.Page = pageNumber;

            _testContext.AccountApi.MockServer
            .Given(
                    Request
                    .Create()
                    .WithPath($"/api/accountlegalentities")
                    .WithParam("pageNumber", new ExactMatcher($"{pageNumber}"))
                    .UsingGet()
                    )
                .RespondWith(Response.Create()
                .WithStatusCode(HttpStatusCode.OK)
                .WithHeader("Content-Type", "application/json")
                .WithBodyAsJson(testData));
        }
    }
}
