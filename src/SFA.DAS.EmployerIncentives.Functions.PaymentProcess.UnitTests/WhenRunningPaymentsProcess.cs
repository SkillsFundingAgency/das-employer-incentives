using AutoFixture;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Application.UnitTests;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Orchestrators;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.DataTransferObjects.Queries.ApprenticeshipIncentives;
using CollectionPeriod = SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.CollectionPeriod;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentProcess.UnitTests
{
    public class WhenRunningPaymentsProcess
    {
        private Fixture _fixture;
        private CollectionPeriod _collectionPeriod;
        private Mock<IDurableOrchestrationContext> _mockOrchestrationContext;
        private IncentivePaymentOrchestrator _orchestrator;
        private List<PayableLegalEntity> _payableLegalEntities;
        private List<ClawbackLegalEntity> _clawbackLegalEntities;

        private Mock<ILogger<IncentivePaymentOrchestrator>> _mockLogger;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();
            _collectionPeriod = _fixture.Create<CollectionPeriod>();
            _mockOrchestrationContext = new Mock<IDurableOrchestrationContext>();

            _payableLegalEntities = _fixture.CreateMany<PayableLegalEntity>(3).ToList();
            _payableLegalEntities[0].AccountId = 123;
            _payableLegalEntities[0].AccountLegalEntityId = 456;
            _clawbackLegalEntities = _fixture.CreateMany<ClawbackLegalEntity>(2).ToList();
            _clawbackLegalEntities[1].AccountId = _payableLegalEntities[0].AccountId;
            _clawbackLegalEntities[1].AccountLegalEntityId = _payableLegalEntities[0].AccountLegalEntityId;

            _mockOrchestrationContext.Setup(x => x.CallActivityAsync<List<PayableLegalEntity>>(nameof(GetPayableLegalEntities), _collectionPeriod)).ReturnsAsync(_payableLegalEntities);            
            _mockOrchestrationContext.Setup(x => x.CallActivityAsync<List<ClawbackLegalEntity>>(nameof(GetUnsentClawbacks), _collectionPeriod)).ReturnsAsync(_clawbackLegalEntities);

            _mockOrchestrationContext.Setup(x => x.CallActivityAsync<CollectionPeriod> (nameof(GetActiveCollectionPeriod), null)).ReturnsAsync(_collectionPeriod);

            _mockLogger = new Mock<ILogger<IncentivePaymentOrchestrator>>();

            _orchestrator = new IncentivePaymentOrchestrator(_mockLogger.Object);
        }

        [Test]
        public async Task Then_the_active_period_is_set_to_in_progress()
        {
            // act
            await _orchestrator.RunOrchestrator(_mockOrchestrationContext.Object);

            // assert
            _mockOrchestrationContext.Verify(x => x.CallActivityAsync("SetActivePeriodToInProgress", null), Times.Once);
        }

        [Test]
        public async Task Then_query_is_called_to_get_payable_legal_entities()
        {
            // act
            await _orchestrator.RunOrchestrator(_mockOrchestrationContext.Object);

            // assert
            _mockOrchestrationContext.Verify(x => x.CallActivityAsync<List<PayableLegalEntity>>("GetPayableLegalEntities", _collectionPeriod), Times.Once);
        }

        [Test]
        public async Task Then_sub_orchestrator_is_called_to_calculate_payments_for_each_legal_entity()
        {
            // act
            await _orchestrator.RunOrchestrator(_mockOrchestrationContext.Object);

            // assert
            _mockOrchestrationContext.Verify(x => x.CallSubOrchestratorAsync(
                "CalculatePaymentsForAccountLegalEntityOrchestrator",
                It.IsAny<AccountLegalEntityCollectionPeriod>()
            ), Times.Exactly(_payableLegalEntities.Count + _clawbackLegalEntities.Count - 1)); // Duplicate account details in payments and clawbacks

            foreach (var entity in _payableLegalEntities)
            {
                _mockOrchestrationContext.Verify(x => x.CallSubOrchestratorAsync(
                        "CalculatePaymentsForAccountLegalEntityOrchestrator",
                        It.Is<AccountLegalEntityCollectionPeriod>(input =>
                            input.AccountLegalEntityId == entity.AccountLegalEntityId &&
                            input.AccountId == entity.AccountId &&
                            input.CollectionPeriod.Period == _collectionPeriod.Period &&
                            input.CollectionPeriod.Year == _collectionPeriod.Year)

                    ), Times.Once);
            }

            foreach (var entity in _clawbackLegalEntities)
            {
                _mockOrchestrationContext.Verify(x => x.CallSubOrchestratorAsync(
                    "CalculatePaymentsForAccountLegalEntityOrchestrator",
                    It.Is<AccountLegalEntityCollectionPeriod>(input =>
                        input.AccountLegalEntityId == entity.AccountLegalEntityId &&
                        input.AccountId == entity.AccountId &&
                        input.CollectionPeriod.Period == _collectionPeriod.Period &&
                        input.CollectionPeriod.Year == _collectionPeriod.Year)

                ), Times.Once);
            }
        }

        [Test]
        public async Task Then_process_pauses_after_generating_payments()
        {
            // act
            await _orchestrator.RunOrchestrator(_mockOrchestrationContext.Object);

            // assert
            _mockOrchestrationContext.Verify(x => x.CallSubOrchestratorAsync(
                "CalculatePaymentsForAccountLegalEntityOrchestrator",
                It.IsAny<AccountLegalEntityCollectionPeriod>()
            ), Times.Exactly(_payableLegalEntities.Count + _clawbackLegalEntities.Count - 1)); // Duplicated account id across lists

            _mockOrchestrationContext.Verify(x => x.WaitForExternalEvent<bool>("PaymentsApproved"), Times.Once);
        }

        [Test]
        public async Task Then_payments_are_not_sent_if_not_approved()
        {
            // arrange
            _mockOrchestrationContext.Setup(x => x.WaitForExternalEvent<bool>("PaymentsApproved")).ReturnsAsync(false);

            // act
            await _orchestrator.RunOrchestrator(_mockOrchestrationContext.Object);

            // assert
            _mockOrchestrationContext.Verify(x => x.CallActivityAsync("SendPaymentRequestsForAccountLegalEntity", It.IsAny<AccountLegalEntityCollectionPeriod>()), Times.Never);
        }

        [Test]
        public async Task Then_payments_and_clawbacks_are_sent_when_they_are_approved()
        {
            // arrange
            _mockOrchestrationContext.Setup(x => x.WaitForExternalEvent<bool>("PaymentsApproved")).ReturnsAsync(true);

            // act
            await _orchestrator.RunOrchestrator(_mockOrchestrationContext.Object);

            // assert
            var expectedCallCount = _payableLegalEntities.Count + _clawbackLegalEntities.Count - 1; // one account legal entity has both payments and clawbacks
            _mockOrchestrationContext.Verify(x => x.CallSubOrchestratorAsync("SendPaymentsForAccountLegalEntityOrchestrator", It.IsAny<AccountLegalEntityCollectionPeriod>()), Times.Exactly(expectedCallCount));

            foreach (var entity in _payableLegalEntities)
            {
                _mockOrchestrationContext.Verify(x => x.CallSubOrchestratorAsync(
                    "SendPaymentsForAccountLegalEntityOrchestrator",
                    It.Is<AccountLegalEntityCollectionPeriod>(input =>
                        input.AccountLegalEntityId == entity.AccountLegalEntityId &&
                        input.AccountId == entity.AccountId &&
                        input.CollectionPeriod.Period == _collectionPeriod.Period &&
                        input.CollectionPeriod.Year == _collectionPeriod.Year)

                ), Times.Once);
            }

            foreach (var entity in _clawbackLegalEntities)
            {
                _mockOrchestrationContext.Verify(x => x.CallSubOrchestratorAsync(
                    "SendPaymentsForAccountLegalEntityOrchestrator",
                    It.Is<AccountLegalEntityCollectionPeriod>(input =>
                        input.AccountLegalEntityId == entity.AccountLegalEntityId &&
                        input.AccountId == entity.AccountId &&
                        input.CollectionPeriod.Period == _collectionPeriod.Period &&
                        input.CollectionPeriod.Year == _collectionPeriod.Year)

                ), Times.Once);
            }
        }

        [Test]
        public async Task Then_call_is_made_to_get_unsent_clawback_legal_entities()
        {
            // act
            await _orchestrator.RunOrchestrator(_mockOrchestrationContext.Object);

            // assert
            _mockOrchestrationContext.Verify(x => x.CallActivityAsync<List<ClawbackLegalEntity>>(nameof(GetUnsentClawbacks), _collectionPeriod), Times.Once);
        }

        [Test]
        public async Task Then_call_is_made_to_get_the_active_collection_period()
        {
            // act
            await _orchestrator.RunOrchestrator(_mockOrchestrationContext.Object);

            // assert
            _mockOrchestrationContext.Verify(x => x.CallActivityAsync<CollectionPeriod>(nameof(GetActiveCollectionPeriod), null), Times.Once);
        }

        [Test]
        public async Task Then_unsent_clawback_legal_entities_are_logged()
        {
            // arrange
            var clawbackLegalEntities = _fixture.Create<List<ClawbackLegalEntity>>();

            _mockOrchestrationContext
                .Setup(m => m.CallActivityAsync<List<ClawbackLegalEntity>>(nameof(GetUnsentClawbacks), _collectionPeriod))
                .ReturnsAsync(clawbackLegalEntities);

            // act 
            await _orchestrator.RunOrchestrator(_mockOrchestrationContext.Object);

            // assert
            foreach (var clawbackLegalEntity in clawbackLegalEntities)
            {
                _mockLogger.VerifyLog(LogLevel.Debug, Times.Once(), $"Unsent clawback for AccountId : {clawbackLegalEntity.AccountId}, AccountLegalEntityId : {clawbackLegalEntity.AccountLegalEntityId}, Collection Year : {_collectionPeriod.Year}, Period : {_collectionPeriod.Period}");
            }            
        }
    }
}