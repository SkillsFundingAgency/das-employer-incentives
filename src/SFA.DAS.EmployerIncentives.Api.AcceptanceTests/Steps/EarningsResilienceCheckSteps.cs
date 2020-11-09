using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Data.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "EarningsResilienceCheck")]
    public class EarningsResilienceCheckSteps : StepsBase
    {
        private readonly TestContext _testContext;
        private readonly Fixture _fixture;

        public EarningsResilienceCheckSteps(TestContext testContext) : base(testContext)
        {
            _testContext = testContext;
            _fixture = new Fixture();
        }

        [Given(@"the earnings resilience check is requested")]
        public void GivenTheEarningsResilienceCheckIsRequested()
        {
            
        }

        [When(@"there are apprenticeships that do not have earnings calculations")]
        public void WhenThereAreApprenticeshipsThatDoNotHaveEarningsCalculations()
        {
            var applications = _fixture.CreateMany<IncentiveApplication>(10);
            foreach(var application in applications)
            {
                DataAccess.SetupApplication(application);
                var apprenticeships = _fixture.CreateMany<IncentiveApplicationApprenticeship>(2);
                application.Apprenticeships = new Collection<IncentiveApplicationApprenticeship>(apprenticeships.ToList());
                foreach(var apprenticeship in apprenticeships)
                {
                    apprenticeship.IncentiveApplicationId = application.Id;
                    DataAccess.SetupApprenticeship(apprenticeship);
                }
            }
        }

        [Then(@"the earnings recalculation is triggered")]
        public async Task ThenTheEarningsRecalculationIsTriggered()
        {
            var url = "earnings-resilience-check";
            var data = string.Empty;
            var apiResult = await EmployerIncentiveApi.Client.PostAsync(url, data.GetStringContent());

            apiResult.StatusCode.Should().Be(HttpStatusCode.OK);

            var publishedCommands = _testContext.CommandsPublished.Where(c => c.IsPublished).Select(c => c.Command)
                .ToArray();
            publishedCommands.Count().Should().Be(10);
            foreach (var publishedCommand in publishedCommands)
            {
                publishedCommand.Should().BeOfType<CreateApprenticeshipIncentiveCommand>();
            }
        }

    }
}
