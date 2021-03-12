using AutoFixture;
using FluentAssertions;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Enums;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "EligibleApprenticeshipRequested")]
    public class EligibleApprenticeshipRequestedSteps : StepsBase
    {
        private HttpResponseMessage _apiResult;
        private readonly long _uln;
        private readonly IncentiveApplication _incentiveApplication;
        private readonly IncentiveApplicationApprenticeship _incentiveApprenticeship;

        public EligibleApprenticeshipRequestedSteps(TestContext testContext) : base(testContext)
        {
            _uln = Fixture.Create<long>();
            _incentiveApplication = Fixture.Build<IncentiveApplication>().Without(x => x.Apprenticeships).Create();
            _incentiveApprenticeship =
                Fixture.Build<IncentiveApplicationApprenticeship>()
                    .With(x => x.ULN, _uln)
                    .With(x => x.IncentiveApplicationId, _incentiveApplication.Id)
                    .With(x => x.WithdrawnByCompliance, false)
                    .With(x => x.WithdrawnByEmployer, false)
                    .Create();

            _incentiveApplication.Apprenticeships.Add(_incentiveApprenticeship);
        }

        [Given(@"I am applying for the New Apprenticeship Incentive")]
        public void GivenIAmApplyingForTheNewApprenticeshipIncentive()
        {
            // intentionally blank
        }

        [Given(@"the ULN has been used on a previously submitted Incentive")]
        public Task GivenTheULNHasBeenUsedOnAPreviouslySubmittedIncentive()
        {
            return SetupApplicationAndApprenticeship(IncentiveApplicationStatus.Submitted);
        }

        [Given(@"the ULN has been used on a draft Incentive Application")]
        public Task GivenTheULNHasBeenUsedOnAPreviouslyInProgressIncentive()
        {
            return SetupApplicationAndApprenticeship(IncentiveApplicationStatus.InProgress);
        }

        [When(@"I request the eligibility of an apprenticeship")]
        public async Task WhenIRequestTheEligibilityOfAnApprenticeship()
        {
            var url = $"eligible-apprenticeships/{_uln}?startDate=2020-08-01&isApproved=true";
            _apiResult = await EmployerIncentiveApi.Client.GetAsync(url);
        }

        [Then(@"the status of the apprenticeship is returned as eligible")]
        public void ThenTheStatusOfTheApprenticeshipIsReturnedAsEligible()
        {
            _apiResult.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Then(@"the status of the apprenticeship is returned as not eligible")]
        public void ThenTheStatusOfTheApprenticeshipIsReturnedAsNotEligible()
        {
            _apiResult.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Given(@"the ULN has been withdrawn by Employer")]
        public async Task GivenTheULNHasBeenWithdrawnByEmployer()
        {
            _incentiveApprenticeship.WithdrawnByEmployer = true;
            await DataAccess.Update(_incentiveApprenticeship);
        }

        [Given(@"the ULN has been withdrawn by Compliance")]
        public async Task GivenTheULNHasBeenWithdrawnByCompliance()
        {
            _incentiveApprenticeship.WithdrawnByCompliance = true;
            await DataAccess.Update(_incentiveApprenticeship);
        }

        private async Task SetupApplicationAndApprenticeship(IncentiveApplicationStatus status)
        {
            _incentiveApplication.Status = status;
            await DataAccess.InsertWithEnumAsString(_incentiveApplication);
            await DataAccess.Insert(_incentiveApprenticeship);
        }
    }
}
