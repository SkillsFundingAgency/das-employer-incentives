using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess;

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
        public void Setup()
        {
            _fixture = new Fixture();
            var collectionPeriod = _fixture.Create<CollectionPeriod>();
            var accountLegalEntityId = _fixture.Create<long>();
            _accountLegalEntityCollectionPeriod = new AccountLegalEntityCollectionPeriod { AccountLegalEntityId = accountLegalEntityId, CollectionPeriod = collectionPeriod };
            _mockOrchestrationContext = new Mock<IDurableOrchestrationContext>();
            _mockOrchestrationContext.Setup(x => x.GetInput<AccountLegalEntityCollectionPeriod>()).Returns(_accountLegalEntityCollectionPeriod);

            _pendingPayments = _fixture.CreateMany<PendingPaymentActivityDto>(3).ToList();
            _mockOrchestrationContext.Setup(x => x.CallActivityAsync<List<PendingPaymentActivityDto>>("GetPendingPaymentsForAccountLegalEntity", _accountLegalEntityCollectionPeriod)).ReturnsAsync(_pendingPayments);

            _orchestrator = new CalculatePaymentsForAccountLegalEntityOrchestrator(Mock.Of<ILogger<CalculatePaymentsForAccountLegalEntityOrchestrator>>());
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
    }
}