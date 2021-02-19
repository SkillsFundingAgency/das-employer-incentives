using AutoFixture;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Application.UnitTests;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Orchestrators;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentProcess.UnitTests
{
    public class WhenRunningPaymentsProcess
    {
        private Fixture _fixture;
        private CollectionPeriod _collectionPeriod;
        private Mock<IDurableOrchestrationContext> _mockOrchestrationContext;
        private IncentivePaymentOrchestrator _orchestrator;
        private List<PayableLegalEntityDto> _legalEntities;
        private Mock<ILogger<IncentivePaymentOrchestrator>> _mockLogger;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();
            _collectionPeriod = _fixture.Create<CollectionPeriod>();
            _mockOrchestrationContext = new Mock<IDurableOrchestrationContext>();
            _mockOrchestrationContext.Setup(x => x.GetInput<CollectionPeriod>()).Returns(_collectionPeriod);

            _legalEntities = _fixture.CreateMany<PayableLegalEntityDto>(3).ToList();
            _mockOrchestrationContext.Setup(x => x.CallActivityAsync<List<PayableLegalEntityDto>>("GetPayableLegalEntities", _collectionPeriod)).ReturnsAsync(_legalEntities);
            _mockOrchestrationContext.Setup(x => x.CallActivityAsync<List<ClawbackLegalEntityDto>>("GetUnsentClawbacks", _collectionPeriod)).ReturnsAsync(new List<ClawbackLegalEntityDto>());
            
            _mockLogger = new Mock<ILogger<IncentivePaymentOrchestrator>>();

            _orchestrator = new IncentivePaymentOrchestrator(_mockLogger.Object);
        }

        [Test]
        public async Task Then_query_is_called_to_get_payable_legal_entities()
        {
            await _orchestrator.RunOrchestrator(_mockOrchestrationContext.Object);

            _mockOrchestrationContext.Verify(x => x.CallActivityAsync<List<PayableLegalEntityDto>>("GetPayableLegalEntities", _collectionPeriod), Times.Once);
        }

        [Test]
        public async Task Then_sub_orchestrator_is_called_to_calculate_payments_for_each_legal_entity()
        {
            await _orchestrator.RunOrchestrator(_mockOrchestrationContext.Object);

            _mockOrchestrationContext.Verify(x => x.CallSubOrchestratorAsync(
                "CalculatePaymentsForAccountLegalEntityOrchestrator",
                It.IsAny<AccountLegalEntityCollectionPeriod>()
            ), Times.Exactly(_legalEntities.Count));

            foreach (var entity in _legalEntities)
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
            await _orchestrator.RunOrchestrator(_mockOrchestrationContext.Object);

            _mockOrchestrationContext.Verify(x => x.CallSubOrchestratorAsync(
                "CalculatePaymentsForAccountLegalEntityOrchestrator",
                It.IsAny<AccountLegalEntityCollectionPeriod>()
            ), Times.Exactly(_legalEntities.Count));

            _mockOrchestrationContext.Verify(x => x.WaitForExternalEvent<bool>("PaymentsApproved"), Times.Once);
        }

        [Test]
        public async Task Then_payments_are_not_sent_if_not_approved()
        {
            _mockOrchestrationContext.Setup(x => x.WaitForExternalEvent<bool>("PaymentsApproved")).ReturnsAsync(false);

            await _orchestrator.RunOrchestrator(_mockOrchestrationContext.Object);

            _mockOrchestrationContext.Verify(x => x.CallActivityAsync("SendPaymentRequestsForAccountLegalEntity", It.IsAny<AccountLegalEntityCollectionPeriod>()), Times.Never);
        }

        [Test]
        public async Task Then_payments_are_sent_when_they_are_approved()
        {
            _mockOrchestrationContext.Setup(x => x.WaitForExternalEvent<bool>("PaymentsApproved")).ReturnsAsync(true);

            await _orchestrator.RunOrchestrator(_mockOrchestrationContext.Object);

            _mockOrchestrationContext.Verify(x => x.CallActivityAsync("SendPaymentRequestsForAccountLegalEntity", It.IsAny<AccountLegalEntityCollectionPeriod>()), Times.Exactly(3));

            foreach (var entity in _legalEntities)
            {
                _mockOrchestrationContext.Verify(x => x.CallActivityAsync(
                    "SendPaymentRequestsForAccountLegalEntity",
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
            await _orchestrator.RunOrchestrator(_mockOrchestrationContext.Object);

            _mockOrchestrationContext.Verify(x => x.CallActivityAsync<List<ClawbackLegalEntityDto>>(nameof(GetUnsentClawbacks), _collectionPeriod), Times.Once);
        }

        [Test]
        public async Task Then_unsent_clawback_legal_entities_are_logged()
        {
            var clawbackLegalEntities = _fixture.Create<List<ClawbackLegalEntityDto>>();

            _mockOrchestrationContext
                .Setup(m => m.CallActivityAsync<List<ClawbackLegalEntityDto>>(nameof(GetUnsentClawbacks), _collectionPeriod))
                .ReturnsAsync(clawbackLegalEntities);

            await _orchestrator.RunOrchestrator(_mockOrchestrationContext.Object);

            foreach(var clawbackLegalEntity in clawbackLegalEntities)
            {
                _mockLogger.VerifyLog(LogLevel.Information, Times.Once(), $"Unsent clawback for AccountId : {clawbackLegalEntity.AccountId}, AccountLegalEntityId : {clawbackLegalEntity.AccountLegalEntityId}, Collection Year : {_collectionPeriod.Year}, Period : {_collectionPeriod.Period}");
            }            
        }
    }
}