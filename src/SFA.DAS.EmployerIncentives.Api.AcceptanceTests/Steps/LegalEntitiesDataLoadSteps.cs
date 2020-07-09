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
        private readonly HttpStatusCode _expectedResult = HttpStatusCode.OK;

        public LegalEntitiesDataLoadSteps(TestContext testContext) : base(testContext) 
        {
            _testContext = testContext;            
        }

        [Given(@"legal entities exist in Managed Apprenticeships")]
        public void GivenLegalEntitiesExistInMa()
        {
            // set up the paged response
            for (int i = 1; i < 3; i++)
            {
                var testData = _testContext.TestData.GetOrCreate<PagedModel<AccountLegalEntity>>($"PagedModel<AccountLegalEntity>_{i}");
                testData.TotalPages = 3;
                testData.Page = i;

                _testContext.AccountApi.MockServer
                .Given(
                        Request
                        .Create()
                        .WithPath($"/api/accountlegalentities")
                        .WithParam("pageNumber", new ExactMatcher($"{i}"))
                        .UsingGet()
                        )
                    .RespondWith(Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithHeader("Content-Type", "application/json")
                    .WithBodyAsJson(testData));

                // set up the repsonses for each legal entity referenced in the paged response
                foreach (AccountLegalEntity legalEntity in testData.Data)
                {
                    _testContext.AccountApi.MockServer
                    .Given(
                       Request
                       .Create()
                       .WithPath($"/api/accounts/{_testContext.HashingService.HashValue(legalEntity.AccountId)}/legalEntities/{legalEntity.LegalEntityId}")
                       .UsingGet()
                       )
                       .RespondWith(Response.Create()
                       .WithStatusCode(HttpStatusCode.OK)
                       .WithHeader("Content-Type", "application/json")
                       .WithBodyAsJson(new GetLegalEntityResponse
                       {
                           LegalEntity = new LegalEntity
                           {
                               LegalEntityId = legalEntity.LegalEntityId,
                               Name = $"Name_{legalEntity.LegalEntityId}"
                           }
                       }));
                }
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

            EmployerIncentiveApi.Response.StatusCode.Should().Be(_expectedResult);
        }

        [Then(@"the legal entities are available in employer incentives")]
        public void ThenTheLegalEntitiesShouldBeAvailableInEmployerIncentives()
        {
            for (int i = 1; i < 3; i++)
            {
                var testData = _testContext.TestData.GetOrCreate<PagedModel<AccountLegalEntity>>($"PagedModel<AccountLegalEntity>_{i}");
                testData.Data.Count.Should().Be(3);

                if (i == 1)
                {
                    var requests = _testContext
                       .AccountApi
                       .MockServer
                       .FindLogEntries(
                           Request
                           .Create()
                           .WithPath(u => u.StartsWith($"/api/accountlegalentities"))
                           .WithParam("pageNumber", true, new[] { $"{i}" })
                           .UsingGet());

                    requests.AsEnumerable().Count().Should().Be(1);

                    foreach (AccountLegalEntity legalEntity in testData.Data)
                    {
                        requests = _testContext
                             .AccountApi
                             .MockServer
                             .FindLogEntries(
                                 Request
                                 .Create()
                                 .WithPath($"/api/accounts/{_testContext.HashingService.HashValue(legalEntity.AccountId)}/legalEntities/{legalEntity.LegalEntityId}")
                                 .UsingGet());

                        requests.AsEnumerable().Count().Should().Be(1);
                    }

                    // Assert the RefreshLegalEntity Events were published
                    var publishedEvents = _testContext.TestData.GetOrCreate<List<RefreshLegalEntityEvent>>();

                    foreach (AccountLegalEntity legalEntity in testData.Data)
                    {
                        var publishedEvent = publishedEvents.SingleOrDefault(e => e.AccountId == legalEntity.AccountId &&
                                                     e.AccountLegalEntityId == legalEntity.AccountLegalEntityId &&
                                                     e.LegalEntityId == legalEntity.LegalEntityId &&
                                                     e.OrganisationName == $"Name_{legalEntity.LegalEntityId}");

                        publishedEvent.Should().NotBeNull();
                    }
                }
                else
                {
                    // Assert the RefreshLegalEntitiesEvent page events were published (for pages 2 +)
                    var publishedEvents = _testContext.TestData.GetOrCreate<List<RefreshLegalEntitiesEvent>>();
                    var publishedEvent = publishedEvents.SingleOrDefault(e => e.PageNumber == i);                    
                    publishedEvent.Should().NotBeNull();                    
                }
            }
        }
    }
}
