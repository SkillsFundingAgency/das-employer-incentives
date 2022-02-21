using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Data.Models;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "BlockAccountLegalEntityForPayments")]
    public class BlockLegalEntityForPaymentsSteps : StepsBase
    {
        private readonly Fixture _fixture;
        private long _accountId1;
        private long _accountLegalEntityId1;
        private long _accountId2;
        private long _accountLegalEntityId2;
        private string _vendorId1;
        private string _vendorId2;
        private DateTime _vendorBlockEndDate1;
        private DateTime _vendorBlockEndDate2;
        private BlockAccountLegalEntityForPaymentsRequest _blockRequest;

        public BlockLegalEntityForPaymentsSteps(TestContext testContext) : base(testContext)
        {
            _fixture = new Fixture();
        }

        [Given(@"there are accounts in employment incentives that have been validated to receive payments")]
        public async Task GivenThereAreAccountsValidatedToReceivePayments()
        {
            _accountId1 = _fixture.Create<long>();
            _accountLegalEntityId1 = _fixture.Create<long>();
            _accountId2 = _fixture.Create<long>();
            _accountLegalEntityId2 = _fixture.Create<long>();

            _vendorId1 = _fixture.Create<string>();
            _vendorId2 = _fixture.Create<string>();

            var account1 = _fixture.Build<Account>()
                .With(x => x.Id, _accountId1)
                .With(x => x.AccountLegalEntityId, _accountLegalEntityId1)
                .With(x => x.VrfVendorId, _vendorId1)
                .Create();
            var account2 = _fixture.Build<Account>()
                .With(x => x.Id, _accountId2)
                .With(x => x.AccountLegalEntityId, _accountLegalEntityId2)
                .With(x => x.VrfVendorId, _vendorId2)
                .Create();

            await DataAccess.SetupAccount(account1);
            await DataAccess.SetupAccount(account2);
        }

        [When(@"a request to block the account legal entities is received")]
        public async Task WhenARequestToBlockTheAccountLegalEntitiesIsReceived()
        {
            _vendorBlockEndDate1 = DateTime.Now.AddDays(28);
            _vendorBlockEndDate2 = DateTime.Now.AddDays(50);

            var url = "legalentities/blockpayments";
            _blockRequest = new BlockAccountLegalEntityForPaymentsRequest
            {
                ServiceRequest = _fixture.Create<ServiceRequest>(),
                VendorBlocks = new List<VendorBlock>
                {
                    new VendorBlock { VendorId = _vendorId1, VendorBlockEndDate = _vendorBlockEndDate1 },
                    new VendorBlock { VendorId = _vendorId2, VendorBlockEndDate = _vendorBlockEndDate2 }
                }
            };
            var apiResult = await EmployerIncentiveApi.Client.PatchAsync(url, _blockRequest.GetStringContent());

            apiResult.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Then(@"the vendor block end dates are set as per the request")]
        public void ThenTheVendorBlockEndDatesAreSetAsPerTheRequest()
        {
            var account1 = DataAccess.GetAccountByAccountLegalEntityId(_accountId1, _accountLegalEntityId1);
            var account2 = DataAccess.GetAccountByAccountLegalEntityId(_accountId2, _accountLegalEntityId2);

            account1.VendorBlockEndDate.Should().Be(_vendorBlockEndDate1);
            account2.VendorBlockEndDate.Should().Be(_vendorBlockEndDate2);
        }
    }
}
