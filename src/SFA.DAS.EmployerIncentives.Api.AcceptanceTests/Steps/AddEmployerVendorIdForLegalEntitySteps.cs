using FluentAssertions;
using SFA.DAS.EmployerIncentives.Data.Models;
using System.Threading.Tasks;
using AutoFixture;
using TechTalk.SpecFlow;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using System;
using Newtonsoft.Json;
using SFA.DAS.EmployerIncentives.Api.Types;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "AddEmployerVendorIdForLegalEntity")]

    public class AddEmployerVendorIdForLegalEntitySteps : StepsBase
    {
        private Account _account1;
        private Account _account2;
        private string _existingVendorId;
        private string _newVendorId;
        private string _hashedLegalEntityId;

        public AddEmployerVendorIdForLegalEntitySteps(TestContext testContext) : base(testContext)
        {
            _existingVendorId = Fixture.Create<string>();
            _newVendorId = Fixture.Create<string>();
            _hashedLegalEntityId = "ABC123";
        }

        [Given(@"a legal entity exists with a vendor assigned within an account")]
        public Task GivenALegalEntityExistsWithAVendorAssignedWithinAnAccount()
        {
            _account1 = TestContext.TestData.GetOrCreate<Account>("FirstAccount");
            _account1.HashedLegalEntityId = _hashedLegalEntityId;
            _account1.VrfVendorId = _existingVendorId;
            return DataAccess.SetupAccount(_account1);
        }

        [Given(@"a the same legal entity exists without a vendor assigned for a seperate account")]
        public Task GivenATheSameLegalEntityExistsWithoutAVendorAssignedForASeperateAccount()
        {
            _account2 = TestContext.TestData.GetOrCreate<Account>("SecondAccount");
            _account2.HashedLegalEntityId = _hashedLegalEntityId;
            _account2.VrfVendorId = null;
            return  DataAccess.SetupAccount(_account2);
        }

        [When(@"we add the employer vendor for this legal entity")]
        public async Task WhenWeAddTheEmployerVendorForThisLegalEntity()
        {
            var url = $"/Vendors";
            var data = new Vendor
            {
                HashedLegalEntityId = _hashedLegalEntityId,
                EmployerVendorId = _newVendorId
            };

             await EmployerIncentiveApi.Post(url, data);
        }

        [Then(@"the vendor remains the same for first legal entity")]
        public void ThenTheVendorRemainsTheSameForFirstLegalEntity()
        {
            var account = DataAccess.GetAccountByAccountLegalEntityId(_account1.Id, _account1.AccountLegalEntityId);
            account.VrfVendorId.Should().Be(_existingVendorId);
        }

        [Then(@"the vendor is updated for the second legal entity")]
        public void ThenTheVendorIsUpdatedForTheSecondLegalEntity()
        {
            var account = DataAccess.GetAccountByAccountLegalEntityId(_account2.Id, _account2.AccountLegalEntityId);
            account.VrfVendorId.Should().Be(_newVendorId);
        }
    }
}
