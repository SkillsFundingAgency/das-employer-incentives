using AutoFixture;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentProcess.UnitTests
{
    public class WhenCalculatingPaymentsForAccountLegalEntity
    {
        private Fixture _fixture;
        private Mock<IDurableOrchestrationContext> _mockOrchestrationContext;
        private CalculatePaymentsForAccountLegalEntityOrchestrator _orchestrator;
        private AccountLegalEntityCollectionPeriod _accountLegalEntityCollectionPeriod;
        private List<PendingPaymentActivityDto> _pendingPayments;

        [SetUp]
        public async Task Setup()
        {
            // Arrange 
            _fixture = new Fixture();
            var collectionPeriod = _fixture.Create<CollectionPeriod>();
            var accountLegalEntityId = _fixture.Create<long>();
            _accountLegalEntityCollectionPeriod = new AccountLegalEntityCollectionPeriod { AccountLegalEntityId = accountLegalEntityId, CollectionPeriod = collectionPeriod };
            _mockOrchestrationContext = new Mock<IDurableOrchestrationContext>();
            _mockOrchestrationContext.Setup(x => x.GetInput<AccountLegalEntityCollectionPeriod>()).Returns(_accountLegalEntityCollectionPeriod);

            _pendingPayments = _fixture.CreateMany<PendingPaymentActivityDto>(3).ToList();
            _mockOrchestrationContext.Setup(x => x.CallActivityAsync<List<PendingPaymentActivityDto>>("GetPendingPaymentsForAccountLegalEntity", _accountLegalEntityCollectionPeriod)).ReturnsAsync(_pendingPayments);

            _orchestrator = new CalculatePaymentsForAccountLegalEntityOrchestrator();
        }

        [Test]
        public async Task Then_query_is_called_to_get_pending_payments_for_the_legal_entity()
        {
            await _orchestrator.RunOrchestrator(_mockOrchestrationContext.Object);

            _mockOrchestrationContext.Verify(x => x.CallActivityAsync<List<PendingPaymentActivityDto>>("GetPendingPaymentsForAccountLegalEntity", _accountLegalEntityCollectionPeriod), Times.Once);
        }

        [Test]
        public async Task Then_payment_is_created_after_validation_completes()
        {
            await _orchestrator.RunOrchestrator(_mockOrchestrationContext.Object);

            _mockOrchestrationContext.Verify(
                x => x.CallActivityAsync<object>("CreatePayment",
                    It.Is<CreatePaymentInput>(y =>
                        y.ApprenticeshipIncentiveId == _pendingPayments[0].ApprenticeshipIncentiveId &&
                        y.PendingPaymentId == _pendingPayments[0].PendingPaymentId && 
                        y.CollectionPeriod == _accountLegalEntityCollectionPeriod.CollectionPeriod)), Times.Once);

            _mockOrchestrationContext.Verify(
                x => x.CallActivityAsync<object>("CreatePayment",
                    It.Is<CreatePaymentInput>(y =>
                        y.ApprenticeshipIncentiveId == _pendingPayments[1].ApprenticeshipIncentiveId &&
                        y.PendingPaymentId == _pendingPayments[1].PendingPaymentId &&
                        y.CollectionPeriod == _accountLegalEntityCollectionPeriod.CollectionPeriod)), Times.Once);

            _mockOrchestrationContext.Verify(
                x => x.CallActivityAsync<object>("CreatePayment",
                    It.Is<CreatePaymentInput>(y =>
                        y.ApprenticeshipIncentiveId == _pendingPayments[2].ApprenticeshipIncentiveId &&
                        y.PendingPaymentId == _pendingPayments[2].PendingPaymentId &&
                        y.CollectionPeriod == _accountLegalEntityCollectionPeriod.CollectionPeriod)), Times.Once);
		}

        [Test]
        public async Task Then_activity_is_called_to_validate_pending_payments_for_the_legal_entity()
        {
            await _orchestrator.RunOrchestrator(_mockOrchestrationContext.Object);

            foreach (var p in _pendingPayments)
                _mockOrchestrationContext.Verify(
                    x => x.CallActivityAsync<object>("ValidatePendingPayment", It.Is<ValidatePendingPaymentData>(
                        d => d.ApprenticeshipIncentiveId == p.ApprenticeshipIncentiveId
                        && d.PendingPaymentId == p.PendingPaymentId
                        && d.Month == _accountLegalEntityCollectionPeriod.CollectionPeriod.Month
                        && d.Year == _accountLegalEntityCollectionPeriod.CollectionPeriod.Year)),
                    Times.Once);
        }
    }
}