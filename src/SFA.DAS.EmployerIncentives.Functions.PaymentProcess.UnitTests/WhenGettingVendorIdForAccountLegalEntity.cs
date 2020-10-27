//using System.Threading.Tasks;
//using AutoFixture;
//using FluentAssertions;
//using Microsoft.Extensions.Logging;
//using Moq;
//using NUnit.Framework;
//using SFA.DAS.EmployerIncentives.Abstractions.DTOs;
//using SFA.DAS.EmployerIncentives.Abstractions.Queries;
//using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess;
//using SFA.DAS.EmployerIncentives.Queries.Account.GetLegalEntity;

//namespace SFA.DAS.EmployerIncentives.Functions.PaymentProcess.UnitTests
//{
//    public class WhenGettingVendorIdForAccountLegalEntity
//    {
//        private Fixture _fixture;
//        private AccountLegalEntityCollectionPeriod _accountLegalEntityCollectionPeriod;
//        private GetVendorIdForAccountLegalEntity _sut;
//        private LegalEntityDto _legalEntity;
//        private Mock<IQueryDispatcher> _mockQueryDispatcher;

//        [SetUp]
//        public void Setup()
//        {
//            _fixture = new Fixture();
//            _accountLegalEntityCollectionPeriod = _fixture.Create<AccountLegalEntityCollectionPeriod>();
//            _legalEntity = _fixture.Create<LegalEntityDto>();

//            _mockQueryDispatcher = new Mock<IQueryDispatcher>();
//            _mockQueryDispatcher
//                .Setup(x =>
//                    x.Send<GetLegalEntityRequest, GetLegalEntityResponse>(
//                        It.Is<GetLegalEntityRequest>(p=>p.AccountId == _accountLegalEntityCollectionPeriod.AccountId && p.AccountLegalEntityId == _accountLegalEntityCollectionPeriod.AccountLegalEntityId)))
//                .ReturnsAsync(new GetLegalEntityResponse { LegalEntity = _legalEntity });

//            _sut = new GetVendorIdForAccountLegalEntity(_mockQueryDispatcher.Object, Mock.Of<ILogger<GetVendorIdForAccountLegalEntity>>());
//        }

//        [Test]
//        public async Task Then_query_is_called_to_get_vendor_id()
//        {
//            await _sut.Get(_accountLegalEntityCollectionPeriod);

//            _mockQueryDispatcher.Verify(
//                x => x.Send<GetLegalEntityRequest, GetLegalEntityResponse>(
//                    It.Is<GetLegalEntityRequest>(p =>
//                        p.AccountLegalEntityId == _accountLegalEntityCollectionPeriod.AccountLegalEntityId &&
//                        p.AccountId == _accountLegalEntityCollectionPeriod.AccountId)), Times.Once);
//        }

//        [TestCase(null)]
//        [TestCase("ABCD1234")]
//        public async Task Then_query_returns_the_vendor_id(string vrfVendorId)
//        {
//            _legalEntity.VrfVendorId = vrfVendorId;

//            var vendorId = await _sut.Get(_accountLegalEntityCollectionPeriod);

//            vendorId.Should().Be(_legalEntity.VrfVendorId);
//        }


//        [Test]
//        public async Task Then_vendor_id_return_null_if_no_account_legal_entity_is_found()
//        {
//            var missingAccountLegalEntity = _fixture.Create<AccountLegalEntityCollectionPeriod>();

//            var vendorId = await _sut.Get(missingAccountLegalEntity);

//            vendorId.Should().BeNull();
//        }
//    }
//}