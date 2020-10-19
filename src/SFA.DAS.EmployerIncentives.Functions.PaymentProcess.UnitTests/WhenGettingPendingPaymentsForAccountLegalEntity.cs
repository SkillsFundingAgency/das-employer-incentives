using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess;
using SFA.DAS.EmployerIncentives.Queries.ApprenticeshipIncentives.GetPendingPaymentsForAccountLegalEntity;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentProcess.UnitTests
{
    public class WhenGettingPendingPaymentsForAccountLegalEntity
    {
        private Fixture _fixture;
        private AccountLegalEntityCollectionPeriod _accountLegalEntityCollectionPeriod;
        private GetPendingPaymentsForAccountLegalEntity _sut;
        private List<PendingPaymentDto> _pendingPayments;
        private Mock<IQueryDispatcher> _mockQueryDispatcher;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();
            _accountLegalEntityCollectionPeriod = _fixture.Create<AccountLegalEntityCollectionPeriod>();
            _pendingPayments = _fixture.CreateMany<PendingPaymentDto>(3).ToList();
            _mockQueryDispatcher = new Mock<IQueryDispatcher>();
            _mockQueryDispatcher
                .Setup(x =>
                    x.Send<GetPendingPaymentsForAccountLegalEntityRequest, GetPendingPaymentsForAccountLegalEntityResponse>(
                        It.IsAny<GetPendingPaymentsForAccountLegalEntityRequest>()))
                .ReturnsAsync(new GetPendingPaymentsForAccountLegalEntityResponse(_pendingPayments));

            _sut = new GetPendingPaymentsForAccountLegalEntity(_mockQueryDispatcher.Object, Mock.Of<ILogger<GetPendingPaymentsForAccountLegalEntity>>());
        }

        [Test]
        public async Task Then_query_is_called_to_get_pending_payments()
        {
            await _sut.Get(_accountLegalEntityCollectionPeriod);

            _mockQueryDispatcher.Verify(
                x => x.Send<GetPendingPaymentsForAccountLegalEntityRequest, GetPendingPaymentsForAccountLegalEntityResponse>(
                    It.Is<GetPendingPaymentsForAccountLegalEntityRequest>(p =>
                        p.AccountLegalEntityId == _accountLegalEntityCollectionPeriod.AccountLegalEntityId &&
                        p.CollectionPeriodMonth == _accountLegalEntityCollectionPeriod.CollectionPeriod.Month &&
                        p.CollectionPeriodYear == _accountLegalEntityCollectionPeriod.CollectionPeriod.Year)), Times.Once);
        }

        [Test]
        public async Task Then_query_returns_list_of_pending_payments()
        {
            var list = await _sut.Get(_accountLegalEntityCollectionPeriod);

            list.Should().BeEquivalentTo(_pendingPayments.Select(x=> new PendingPaymentActivityDto { PendingPaymentId = x.Id, ApprenticeshipIncentiveId = x.ApprenticeshipIncentiveId}).ToList());
        }
    }
}