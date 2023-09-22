using System;
using System.Collections.Generic;
using System.Linq;
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
        private long _accountId2;
        private long _accountLegalEntityId1;
        private long _accountLegalEntityId2;
        private long _accountLegalEntityId3;
        private long _accountLegalEntityId4;
        private List<BlockAccountLegalEntityForPaymentsRequest> _blockRequest;
        private DateTime _vendorBlockEndDate1;
        private DateTime _vendorBlockEndDate2;
        private string _vendorId1;
        private string _vendorId2;
        private string _vendorId3;

        public BlockLegalEntityForPaymentsSteps(TestContext testContext) : base(testContext) => _fixture = new Fixture();

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
                .Without(x => x.VendorBlockEndDate)
                .Create();

            var account2 = _fixture.Build<Account>()
                .With(x => x.Id, _accountId2)
                .With(x => x.AccountLegalEntityId, _accountLegalEntityId2)
                .With(x => x.VrfVendorId, _vendorId2)
                .Without(x => x.VendorBlockEndDate)
                .Create();

            await DataAccess.SetupAccount(account1);
            await DataAccess.SetupAccount(account2);
        }

        [Given("there are multiple accounts with the same vendor ID that have been validated to receive payments")]
        public async Task GivenThereAreMultipleAccountsWithTheSameVendorIdThatHaveBeenValidatedToReceivePayments()
        {
            _accountId1 = _fixture.Create<long>();
            _accountLegalEntityId1 = _fixture.Create<long>();
            _accountId2 = _fixture.Create<long>();
            _accountLegalEntityId2 = _fixture.Create<long>();
            _accountLegalEntityId3 = _fixture.Create<long>();
            _accountLegalEntityId4 = _fixture.Create<long>();

            _vendorId1 = _fixture.Create<string>();

            var account1 = _fixture.Build<Account>()
                .With(x => x.Id, _accountId1)
                .With(x => x.AccountLegalEntityId, _accountLegalEntityId1)
                .With(x => x.VrfVendorId, _vendorId1)
                .Without(x => x.VendorBlockEndDate)
                .Create();

            var account2 = _fixture.Build<Account>()
                .With(x => x.Id, _accountId2)
                .With(x => x.AccountLegalEntityId, _accountLegalEntityId2)
                .With(x => x.VrfVendorId, _vendorId1)
                .Without(x => x.VendorBlockEndDate)
                .Create();

            var account3 = _fixture.Build<Account>()
                .With(x => x.Id, _accountId1)
                .With(x => x.AccountLegalEntityId, _accountLegalEntityId3)
                .With(x => x.VrfVendorId, _vendorId2)
                .Without(x => x.VendorBlockEndDate)
                .Create();

            var account4 = _fixture.Build<Account>()
                .With(x => x.Id, _accountId2)
                .With(x => x.AccountLegalEntityId, _accountLegalEntityId4)
                .With(x => x.VrfVendorId, _vendorId3)
                .Without(x => x.VendorBlockEndDate)
                .Create();

            await DataAccess.SetupAccount(account1);
            await DataAccess.SetupAccount(account2);
            await DataAccess.SetupAccount(account3);
            await DataAccess.SetupAccount(account4);
        }

        [When(@"a request to block the account legal entities is received")]
        public async Task WhenARequestToBlockTheAccountLegalEntitiesIsReceived()
        {
            _vendorBlockEndDate1 = DateTime.Now.AddDays(28);
            _vendorBlockEndDate2 = DateTime.Now.AddDays(50);

            var url = "/blockedpayments";
            _blockRequest = new List<BlockAccountLegalEntityForPaymentsRequest>
            {
                new BlockAccountLegalEntityForPaymentsRequest
                {
                    ServiceRequest = _fixture.Create<ServiceRequest>(),
                    VendorBlocks = new List<VendorBlock>
                    {
                        new VendorBlock
                        {
                            VendorId = _vendorId1,
                            VendorBlockEndDate = _vendorBlockEndDate1
                        },
                        new VendorBlock
                        {
                            VendorId = _vendorId2,
                            VendorBlockEndDate = _vendorBlockEndDate2
                        }
                    }
                }
            };


            var apiResult = await EmployerIncentiveApi.Client.PatchAsync(url, _blockRequest.GetStringContent());

            apiResult.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [When("a request to block a single vendor ID is received")]
        public async Task WhenARequestToBlockASingleVendorIdIsReceived()
        {
            _vendorBlockEndDate1 = DateTime.Now.AddDays(28);

            var url = "/blockedpayments";
            _blockRequest = new List<BlockAccountLegalEntityForPaymentsRequest>
            {
                new BlockAccountLegalEntityForPaymentsRequest
                {
                    ServiceRequest = _fixture.Create<ServiceRequest>(),
                    VendorBlocks = new List<VendorBlock>
                    {
                        new VendorBlock
                        {
                            VendorId = _vendorId1,
                            VendorBlockEndDate = _vendorBlockEndDate1
                        }
                    }
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

        [Given("there is an account with multiple legal entities that has been validated to receive payments")]
        public async Task GivenThereIsAnAccountWithMulitpleLegalEntitiesThatHasBeenValidatedToReceivePayments()
        {
            _accountId1 = _fixture.Create<long>();
            _accountLegalEntityId1 = _fixture.Create<long>();
            _accountLegalEntityId2 = _fixture.Create<long>();
            _accountLegalEntityId3 = _fixture.Create<long>();

            _vendorId1 = _fixture.Create<string>();
            _vendorId2 = _fixture.Create<string>();
            _vendorId3 = _fixture.Create<string>();

            var account1 = _fixture.Build<Account>()
                .With(x => x.Id, _accountId1)
                .With(x => x.AccountLegalEntityId, _accountLegalEntityId1)
                .With(x => x.VrfVendorId, _vendorId1)
                .Without(x => x.VendorBlockEndDate)
                .Create();

            var account2 = _fixture.Build<Account>()
                .With(x => x.Id, _accountId1)
                .With(x => x.AccountLegalEntityId, _accountLegalEntityId2)
                .With(x => x.VrfVendorId, _vendorId2)
                .Without(x => x.VendorBlockEndDate)
                .Create();

            var account3 = _fixture.Build<Account>()
                .With(x => x.Id, _accountId1)
                .With(x => x.AccountLegalEntityId, _accountLegalEntityId3)
                .With(x => x.VrfVendorId, _vendorId3)
                .Without(x => x.VendorBlockEndDate)
                .Create();

            await DataAccess.SetupAccount(account1);
            await DataAccess.SetupAccount(account2);
            await DataAccess.SetupAccount(account3);
        }

        [When("a request to block one of the account legal entities is received")]
        public async Task WhenARequestToBlockOneOfTheAccountLegalEntitiesIsReceived()
        {
            _vendorBlockEndDate1 = DateTime.Now.AddDays(28);

            var url = "/blockedpayments";
            _blockRequest = new List<BlockAccountLegalEntityForPaymentsRequest>
            {
                new BlockAccountLegalEntityForPaymentsRequest
                {
                    ServiceRequest = _fixture.Create<ServiceRequest>(),
                    VendorBlocks = new List<VendorBlock>
                    {
                        new VendorBlock
                        {
                            VendorId = _vendorId2,
                            VendorBlockEndDate = _vendorBlockEndDate1
                        }
                    }
                }
            };

            var apiResult = await EmployerIncentiveApi.Client.PatchAsync(url, _blockRequest.GetStringContent());

            apiResult.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Then("the vendor block end date is set for the single legal entity requested")]
        public void ThenTheVendorBlockEndDateIsSetForTheSingleLegalEntityRequested()
        {
            var accounts = DataAccess.GetAccountsById(_accountId1).ToList();
            var updatedAccount = accounts.FirstOrDefault(x => x.AccountLegalEntityId == _accountLegalEntityId2);
            updatedAccount.VendorBlockEndDate.Should().Be(_vendorBlockEndDate1);
        }

        [Then("the legal entities not matching the vendor ID are unmodified")]
        public void ThenTheLegalEntitiesNotMatchingTheVendorIdAreUnModified()
        {
            var accounts = DataAccess.GetAccountsById(_accountId1).ToList();
            accounts.Count.Should().Be(3);
            var firstAccount = accounts.FirstOrDefault(x => x.AccountLegalEntityId == _accountLegalEntityId1);
            var thirdAccount = accounts.FirstOrDefault(x => x.AccountLegalEntityId == _accountLegalEntityId3);
            firstAccount.Should().NotBeNull();
            firstAccount.VendorBlockEndDate.Should().BeNull();
            thirdAccount.Should().NotBeNull();
            thirdAccount.VendorBlockEndDate.Should().BeNull();
        }

        [Then("the vendor block end dates are set for all accounts with the same vendor ID")]
        public void ThenTheVendorBlockEndDatesAreSetForAllAccountsWithTheSameVendorId()
        {
            var accounts1 = DataAccess.GetAccountsById(_accountId1).ToList();
            var accounts2 = DataAccess.GetAccountsById(_accountId2).ToList();

            var matchingAccount1 = accounts1.FirstOrDefault(x => x.VrfVendorId == _vendorId1);
            var matchingAccount2 = accounts2.FirstOrDefault(x => x.VrfVendorId == _vendorId1);

            matchingAccount1.VendorBlockEndDate.Should().Be(_vendorBlockEndDate1);
            matchingAccount2.VendorBlockEndDate.Should().Be(_vendorBlockEndDate1);
        }

        [Then("the legal entities for those accounts not matching the vendor ID are unmodified")]
        public void ThenTheLegalEntitiesForThoseAccountsNotMatchingTheVendorIdAreUnmodified()
        {
            var accounts1 = DataAccess.GetAccountsById(_accountId1).ToList();
            var accounts2 = DataAccess.GetAccountsById(_accountId2).ToList();

            accounts1.Count.Should().Be(2);
            accounts2.Count.Should().Be(2);

            var unmatchedAccount1 = accounts1.FirstOrDefault(x => x.VrfVendorId == _vendorId2);
            var unmatchedAccount2 = accounts2.FirstOrDefault(x => x.VrfVendorId == _vendorId2);

            unmatchedAccount1.VendorBlockEndDate.Should().BeNull();
            unmatchedAccount2.VendorBlockEndDate.Should().BeNull();
        }
    }
}