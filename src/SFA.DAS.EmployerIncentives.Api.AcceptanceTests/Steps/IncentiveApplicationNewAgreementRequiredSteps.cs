using AutoFixture;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using SFA.DAS.EmployerIncentives.DataTransferObjects.Queries;
using TechTalk.SpecFlow;
using IncentiveApplication = SFA.DAS.EmployerIncentives.DataTransferObjects.Queries.IncentiveApplication;
using IncentiveApplicationApprenticeship = SFA.DAS.EmployerIncentives.Data.Models.IncentiveApplicationApprenticeship;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "IncentiveApplicationNewAgreementRequired")]
    public class IncentiveApplicationNewAgreementRequiredSteps : StepsBase
    {
        private Account _account;
        private List<IncentiveApplicationApprenticeship> _apprenticeships;
        private Data.Models.IncentiveApplication _application;
        private IncentiveApplication _savedApplication;

        public IncentiveApplicationNewAgreementRequiredSteps(TestContext context) : base(context)
        {
        }

        [Given(@"an employer who has previously signed a Phase2 agreement version")]
        public async Task GivenAnEmployerWhoHasPreviouslySignedAPhaseAgreementVersion()
        {
            _account = Fixture.Build<Account>().With(a => a.SignedAgreementVersion, 6).Create();
            await DataAccess.Insert(_account);
        }

        [Given(@"the employer has selected the apprenticeships for their application")]
        public async Task GivenTheEmployerHasSelectedTheApprenticeshipsForTheirApplication()
        {
            _application = Fixture.Build<Data.Models.IncentiveApplication>()
                .With(a => a.AccountId, _account.Id)
                .With(a => a.AccountLegalEntityId, _account.AccountLegalEntityId)
                .With(a => a.Status, IncentiveApplicationStatus.Submitted)
                .Without(a => a.Apprenticeships)
                .Create();
            
            await DataAccess.InsertWithEnumAsString(_application);

            _apprenticeships = Fixture.Build<IncentiveApplicationApprenticeship>()
                .With(a => a.IncentiveApplicationId, _application.Id)
                .With(a => a.WithdrawnByCompliance, false)
                .With(a => a.WithdrawnByEmployer, false)
                .With(a => a.EarningsCalculated, false)
                .With(a => a.StartDatesAreEligible, true)
                .Without(a => a.Phase)
                .With(a => a.PlannedStartDate, new DateTime(2021, 10, 1))
                .Without(a => a.EmploymentStartDate)
                .CreateMany(2).ToList();
        }
        
        [Given(@"submitted employment dates for the apprenticeships")]
        public void GivenSubmittedEmploymentDatesForTheApprenticeships()
        {
            // blank
        }

        [Given(@"one of the apprenticeships employment date falls into Phase3 window")]
        public async Task GivenOneOfTheApprenticeshipsEmploymentDateFallsIntoPhaseWindow()
        {
            _apprenticeships[0].EmploymentStartDate = new DateTime(2021, 9, 1); // Phase2
            _apprenticeships[0].Phase = Phase.Phase2; // Phase2
            _apprenticeships[1].EmploymentStartDate = new DateTime(2021, 10, 1); // Phase3
            _apprenticeships[1].Phase = Phase.Phase3; // Phase3

            foreach (var apprenticeship in _apprenticeships)
            {
                await DataAccess.InsertWithEnumAsString(apprenticeship);
            }
        }

        [Given(@"all of the apprenticeships employment dates fall into Phase2 window")]
        public async Task GivenAllOfTheApprenticeshipsEmploymentDatesFallIntoPhaseWindow()
        {
            _apprenticeships[0].EmploymentStartDate = new DateTime(2021, 8, 1); // Phase2
            _apprenticeships[0].Phase = Phase.Phase2; // Phase2
            _apprenticeships[1].EmploymentStartDate = new DateTime(2021, 9, 1); // Phase2
            _apprenticeships[1].Phase = Phase.Phase2; // Phase3

            foreach (var apprenticeship in _apprenticeships)
            {
                await DataAccess.InsertWithEnumAsString(apprenticeship);
            }
        }

        [When(@"they retrieve the application")]
        public async Task WhenTheyRetrieveTheApplication()
        {
            var url = $"/accounts/{_application.AccountId}/applications/{_application.Id}";
            var (_, data) = await EmployerIncentiveApi.Client.GetValueAsync<IncentiveApplication>(url);

            _savedApplication = data;
        }
        
        [Then(@"the employer is asked to sign a new agreement version")]
        public void ThenTheEmployerIsAskedToSignANewAgreementVersion()
        {
            _savedApplication.NewAgreementRequired.Should().BeTrue();
        }

        [Then(@"the employer is not asked to sign a new agreement version")]
        public void ThenTheEmployerIsNotAskedToSignANewAgreementVersion()
        {
            _savedApplication.NewAgreementRequired.Should().BeFalse();
        }
    }
}
