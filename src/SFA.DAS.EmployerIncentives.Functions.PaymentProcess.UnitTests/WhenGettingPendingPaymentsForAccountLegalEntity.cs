using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using SFA.DAS.EmployerIncentives.DataTransferObjects;
using SFA.DAS.EmployerIncentives.DataTransferObjects.Queries.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess;
using SFA.DAS.EmployerIncentives.Queries.ApprenticeshipIncentives.GetPendingPaymentsForAccountLegalEntity;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentProcess.UnitTests
{
    public class WhenGettingPendingPaymentsForAccountLegalEntity
    {
        private Fixture _fixture;
        private AccountLegalEntityCollectionPeriod _accountLegalEntityCollectionPeriod;
        private GetPendingPaymentsForAccountLegalEntity _sut;
        private List<PendingPayment> _pendingPayments;
        private Mock<IQueryDispatcher> _mockQueryDispatcher;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();
            _accountLegalEntityCollectionPeriod = _fixture.Create<AccountLegalEntityCollectionPeriod>();
            _pendingPayments = _fixture.CreateMany<PendingPayment>(3).ToList();
            _mockQueryDispatcher = new Mock<IQueryDispatcher>();
            _mockQueryDispatcher
                .Setup(x =>
                    x.Send<GetPendingPaymentsForAccountLegalEntityRequest, GetPendingPaymentsForAccountLegalEntityResponse>(
                        It.IsAny<GetPendingPaymentsForAccountLegalEntityRequest>()))
                .ReturnsAsync(new GetPendingPaymentsForAccountLegalEntityResponse(_pendingPayments));

            _sut = new GetPendingPaymentsForAccountLegalEntity(_mockQueryDispatcher.Object);
        }

        [Test]
        public async Task Then_query_is_called_to_get_pending_payments()
        {
            await _sut.Get(_accountLegalEntityCollectionPeriod);

            _mockQueryDispatcher.Verify(
                x => x.Send<GetPendingPaymentsForAccountLegalEntityRequest, GetPendingPaymentsForAccountLegalEntityResponse>(
                    It.Is<GetPendingPaymentsForAccountLegalEntityRequest>(p =>
                        p.AccountLegalEntityId == _accountLegalEntityCollectionPeriod.AccountLegalEntityId &&
                        p.CollectionPeriod.PeriodNumber == _accountLegalEntityCollectionPeriod.CollectionPeriod.Period &&
                        p.CollectionPeriod.AcademicYear == _accountLegalEntityCollectionPeriod.CollectionPeriod.Year)), Times.Once);
        }

        [Test]
        public async Task Then_query_returns_list_of_pending_payments()
        {
            var list = await _sut.Get(_accountLegalEntityCollectionPeriod);

            list.Should().BeEquivalentTo(_pendingPayments.Select(x=> new PendingPaymentActivity { PendingPaymentId = x.Id, ApprenticeshipIncentiveId = x.ApprenticeshipIncentiveId}).ToList());
        }
    }
}