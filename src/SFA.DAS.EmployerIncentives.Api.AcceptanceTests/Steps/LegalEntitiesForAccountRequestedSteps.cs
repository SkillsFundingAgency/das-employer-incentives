using AutoFixture;
using FluentAssertions;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs;
using SFA.DAS.EmployerIncentives.Data.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "LegalEntitiesForAccountRequested")]
    public class LegalEntitiesForAccountRequestedSteps : StepsBase
    {
        private IEnumerable<LegalEntityDto> _getLegalEntitiesResponse;
        private long _accountId;
        private readonly Fixture _fixture;
        private Account _accountWithSignedPhase2Agreement;
        private Account _accountWithUnsignedPhase2Agreement;

        public LegalEntitiesForAccountRequestedSteps(TestContext testContext) : base(testContext)
        {
            _fixture = new Fixture();
        }

        [Given(@"an existing Employer Incentives account")]
        public void GivenAnExistingEmployerIncentivesAccount()
        {
            _accountId = _fixture.Create<long>();
        }

        [Given(@"a legal entity who signed version (.*) of the agreement")]
        public async Task GivenALegalEntityWhoSignedVersionOfTheAgreement(int signedAgreementVersion)
        {
            var account = _fixture
                    .Build<Account>()
                    .With(x => x.Id, _accountId)
                    .With(x => x.SignedAgreementVersion, signedAgreementVersion)
                    .Create();
        
            await DataAccess.Insert(account);

            if (signedAgreementVersion == 6)
                _accountWithSignedPhase2Agreement = account;
            else
                _accountWithUnsignedPhase2Agreement = account;
        }


        [When(@"a client requests the legal entities for the account")]
        public async Task WhenAClientRequestsTheLegalEntitiesForTheAccount()
        {
            var url = $"/accounts/{_accountId}/LegalEntities";
            var (status, data) = 
                await EmployerIncentiveApi.Client.GetValueAsync<IEnumerable<LegalEntityDto>>(url);
            
            status.Should().Be(HttpStatusCode.OK);

            _getLegalEntitiesResponse = data;
        }

        [Then(@"the legal entities are returned")]
        public void ThenTheLegalEntitiesAreReturned()
        {
            _getLegalEntitiesResponse.Should().NotBeEmpty();
            _getLegalEntitiesResponse.Single(x => x.HashedLegalEntityId == _accountWithSignedPhase2Agreement.HashedLegalEntityId).AccountLegalEntityId.Should()
                .Be(_accountWithSignedPhase2Agreement.AccountLegalEntityId);
            _getLegalEntitiesResponse.Single(x => x.HashedLegalEntityId == _accountWithUnsignedPhase2Agreement.HashedLegalEntityId).AccountLegalEntityId.Should()
                .Be(_accountWithUnsignedPhase2Agreement.AccountLegalEntityId);
        }

        [Then(@"a property is set indicating whether the minimum required agreement version is signed")]
        public void ThenAPropertyIsSetIndicatingWhetherTheMinimumAgreementVersionIsSigned()
        {
            _getLegalEntitiesResponse.Single(x => x.HashedLegalEntityId == _accountWithSignedPhase2Agreement.HashedLegalEntityId).IsAgreementSigned.Should()
                .BeTrue();
            _getLegalEntitiesResponse.Single(x => x.HashedLegalEntityId == _accountWithUnsignedPhase2Agreement.HashedLegalEntityId).IsAgreementSigned.Should()
                .BeFalse();
        }
    }
}
