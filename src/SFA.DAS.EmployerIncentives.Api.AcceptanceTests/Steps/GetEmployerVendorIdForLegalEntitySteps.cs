using AutoFixture;
using FluentAssertions;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Queries.Account.GetLegalEntity;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "GetEmployerVendorIdForLegalEntity")]
    public class GetEmployerVendorIdForLegalEntitySteps : StepsBase
    {
        private Account _account;
        private string _hashedLegalEntityId;
        private string _vendorId;

        public GetEmployerVendorIdForLegalEntitySteps(TestContext testContext) : base(testContext)
        {
           
        }

        [Given(@"a legal entity exists within an account")]
        public void GivenALegalEntityExistsWithinAnAccount()
        {
            _hashedLegalEntityId = "PVP9DM";
            _account = TestContext.TestData.GetOrCreate<Account>("Account");
            _account.HashedLegalEntityId = _hashedLegalEntityId;            
        }

        [Given(@"the legal entity has a vendor id assigned")]
        public void GivenTheLegalEntityHasAVendorIdAssigned()
        {
            _vendorId = "ABC123";
            _account.VrfVendorId = _vendorId;
            DataAccess.SetupAccount(_account);
        }

        [Then(@"the vendor id associated with the account legal entity is returned")]
        public async Task ThenTheVendorIdAssociatedWithTheAccountLegalEntityIsReturned()
        {
            var url = $"/legalentities?hashedLegalEntityId={_hashedLegalEntityId}";

            var (status, data) =
                await EmployerIncentiveApi.Client.GetValueAsync<GetLegalEntityResponse>(url);

            status.Should().Be(HttpStatusCode.OK);

            data.LegalEntity.VrfVendorId.Should().Be(_vendorId);
        }

        [Given(@"the legal entity does not have a vendor id assigned")]
        public void GivenTheLegalEntityDoesNotHaveAVendorIdAssigned()
        {
            _account.VrfVendorId = null;
            DataAccess.SetupAccount(_account);
        }

        [Then(@"no vendor id is returned")]
        public async Task ThenNoVendorIdIsReturned()
        {
            var url = $"/legalentities?hashedLegalEntityId={_hashedLegalEntityId}";

            var (status, data) =
                await EmployerIncentiveApi.Client.GetValueAsync<GetLegalEntityResponse>(url);

            status.Should().Be(HttpStatusCode.OK);

            data.LegalEntity.VrfVendorId.Should().BeNull();
        }

    }
}
