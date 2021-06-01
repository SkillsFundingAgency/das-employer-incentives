using AutoFixture;
using FluentAssertions;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Data.Models;
using System.Linq;
using System.Net;
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

        [Given(@"there are apprenticeships that do not have earnings calculations")]
        public async Task GivenThereAreApprenticeshipsThatDoNotHaveEarningsCalculations()
        {
            var applications = _fixture.Build<IncentiveApplication>()
                .With(a => a.Status, Enums.IncentiveApplicationStatus.Submitted)
                .CreateMany(10);

            foreach (var application in applications)
            {
                await DataAccess.InsertWithEnumAsString(application);

                var apprenticeships = _fixture.Build<IncentiveApplicationApprenticeship>()
                    .With(a => a.IncentiveApplicationId, application.Id)
                    .With(a => a.WithdrawnByCompliance, false)
                    .With(a => a.WithdrawnByEmployer, false)
                    .With(a => a.EarningsCalculated, false)
                    .With(a => a.HasEligibleEmploymentStartDate, true)
                    .CreateMany(2).ToList();

                foreach (var apprenticeship in apprenticeships)
                {
                    await DataAccess.Insert(apprenticeship);
                }
            }
        }

        [When(@"the earnings resilience check is requested")]
        public async Task WhenTheEarningsResilienceCheckIsRequested()
        {
            var url = "earnings-resilience-check";
            var data = string.Empty;
            var apiResult = await EmployerIncentiveApi.Client.PostAsync(url, data.GetStringContent());

            apiResult.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Then(@"the earnings recalculation is triggered")]
        public void ThenTheEarningsRecalculationIsTriggered()
        {
            _testContext.CommandsPublished.Where(c => 
                    c.IsPublished &&
                    c.Command is CreateIncentiveCommand)
                .Select(c => c.Command)
                .Count().Should().Be(20);
        }

        [Given(@"apprenticeships have been withdrawn by employer")]
        public async Task GivenApprenticeshipsHaveBeenWithdrawnByEmployer()
        {
            var applications = _fixture.Build<IncentiveApplication>()
                .With(a => a.Status, Enums.IncentiveApplicationStatus.Submitted)
                .CreateMany(10);

            foreach (var application in applications)
            {
                await DataAccess.InsertWithEnumAsString(application);

                var apprenticeships = _fixture.Build<IncentiveApplicationApprenticeship>()
                    .With(a => a.IncentiveApplicationId, application.Id)
                    .With(a => a.WithdrawnByCompliance, false)
                    .With(a => a.WithdrawnByEmployer, true)
                    .With(a => a.EarningsCalculated, false)
                    .CreateMany(2).ToList();

                foreach (var apprenticeship in apprenticeships)
                {
                    await DataAccess.Insert(apprenticeship);
                }
            }
        }

        [Then(@"the earnings recalculation is not triggered")]
        public void ThenTheEarningsRecalculationIsNotTriggered()
        {
            _testContext.CommandsPublished.Where(c => c.IsPublished && c.Command is CreateIncentiveCommand)
                .Select(c => c.Command)
                .Any().Should().BeFalse();
        }

        [Given(@"apprenticeships have been withdrawn by compliance")]
        public async Task GivenApprenticeshipsHaveBeenWithdrawnByCompliance()
        {
            var applications = _fixture.Build<IncentiveApplication>()
                .With(a => a.Status, Enums.IncentiveApplicationStatus.Submitted)
                .CreateMany(10);

            foreach (var application in applications)
            {
                await DataAccess.InsertWithEnumAsString(application);

                var apprenticeships = _fixture.Build<IncentiveApplicationApprenticeship>()
                    .With(a => a.IncentiveApplicationId, application.Id)
                    .With(a => a.WithdrawnByCompliance, true)
                    .With(a => a.WithdrawnByEmployer, false)
                    .With(a => a.EarningsCalculated, false)
                    .CreateMany(2).ToList();

                foreach (var apprenticeship in apprenticeships)
                {
                    await DataAccess.Insert(apprenticeship);
                }
            }
        }

    }
}
