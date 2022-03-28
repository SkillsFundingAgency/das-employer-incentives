using AutoFixture;
using FluentAssertions;
using SFA.DAS.EmployerIncentives.Data.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.DataTransferObjects;
using TechTalk.SpecFlow;
using Account = SFA.DAS.EmployerIncentives.Data.Models.Account;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "LegalEntitiesForAccountRequested")]
    public class LegalEntitiesForAccountRequestedSteps : StepsBase
    {
        private IEnumerable<LegalEntity> _getLegalEntitiesResponse;
        private long _accountId;
        private readonly Fixture _fixture;
        private Account _accountWithSignedPhase3Agreement;
        private Account _accountWithUnsignedPhase3Agreement;

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

            if (signedAgreementVersion == 7)
                _accountWithSignedPhase3Agreement = account;
            else
                _accountWithUnsignedPhase3Agreement = account;
        }


        [When(@"a client requests the legal entities for the account")]
        public async Task WhenAClientRequestsTheLegalEntitiesForTheAccount()
        {
            var url = $"/accounts/{_accountId}/LegalEntities";
            var (status, data) = 
                await EmployerIncentiveApi.Client.GetValueAsync<IEnumerable<LegalEntity>>(url);
            
            status.Should().Be(HttpStatusCode.OK);

            _getLegalEntitiesResponse = data;
        }

        [Then(@"the legal entities are returned")]
        public void ThenTheLegalEntitiesAreReturned()
        {
            _getLegalEntitiesResponse.Should().NotBeEmpty();
            _getLegalEntitiesResponse.Single(x => x.HashedLegalEntityId == _accountWithSignedPhase3Agreement.HashedLegalEntityId).AccountLegalEntityId.Should()
                .Be(_accountWithSignedPhase3Agreement.AccountLegalEntityId);
            _getLegalEntitiesResponse.Single(x => x.HashedLegalEntityId == _accountWithUnsignedPhase3Agreement.HashedLegalEntityId).AccountLegalEntityId.Should()
                .Be(_accountWithUnsignedPhase3Agreement.AccountLegalEntityId);
        }

        [Then(@"a property is set indicating whether the minimum required agreement version is signed")]
        public void ThenAPropertyIsSetIndicatingWhetherTheMinimumAgreementVersionIsSigned()
        {
            _getLegalEntitiesResponse.Single(x => x.HashedLegalEntityId == _accountWithSignedPhase3Agreement.HashedLegalEntityId).IsAgreementSigned.Should()
                .BeTrue();
            _getLegalEntitiesResponse.Single(x => x.HashedLegalEntityId == _accountWithUnsignedPhase3Agreement.HashedLegalEntityId).IsAgreementSigned.Should()
                .BeFalse();
        }
    }
}
