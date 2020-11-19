using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentProcess.UnitTests
{
    public class WhenRunningPaymentsProcess
    {
        private Fixture _fixture;
        private CollectionPeriod _collectionPeriod;
        private Mock<IDurableOrchestrationContext> _mockOrchestrationContext;
        private IncentivePaymentOrchestrator _orchestrator;
        private List<long> _legalEntities;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();
            _collectionPeriod = _fixture.Create<CollectionPeriod>();
            _mockOrchestrationContext = new Mock<IDurableOrchestrationContext>();
            _mockOrchestrationContext.Setup(x => x.GetInput<CollectionPeriod>()).Returns(_collectionPeriod);

            _legalEntities = _fixture.CreateMany<long>(3).ToList();
            _mockOrchestrationContext.Setup(x => x.CallActivityAsync<List<long>>("GetPayableLegalEntities", _collectionPeriod)).ReturnsAsync(_legalEntities);

            _orchestrator = new IncentivePaymentOrchestrator(Mock.Of<ILogger<IncentivePaymentOrchestrator>>());
        }

        [Test]
        public async Task Then_query_is_called_to_get_payable_legal_entities()
        {
            await _orchestrator.RunOrchestrator(_mockOrchestrationContext.Object);

            _mockOrchestrationContext.Verify(x => x.CallActivityAsync<List<long>>("GetPayableLegalEntities", _collectionPeriod), Times.Once);
        }

        [Test]
        public async Task Then_sub_orchestrator_is_called_to_calculate_payments_for_each_legal_entity()
        {
            await _orchestrator.RunOrchestrator(_mockOrchestrationContext.Object);

            _mockOrchestrationContext.Verify(x => x.CallSubOrchestratorAsync<object>("CalculatePaymentsForAccountLegalEntityOrchestrator", null, It.Is<object>(y => VerifyInputMatchesAccountAndCollectionPeriod(y, _legalEntities[0]))), Times.Once);
            _mockOrchestrationContext.Verify(x => x.CallSubOrchestratorAsync<object>("CalculatePaymentsForAccountLegalEntityOrchestrator", null, It.Is<object>(y => VerifyInputMatchesAccountAndCollectionPeriod(y, _legalEntities[1]))), Times.Once);
            _mockOrchestrationContext.Verify(x => x.CallSubOrchestratorAsync<object>("CalculatePaymentsForAccountLegalEntityOrchestrator", null, It.Is<object>(y => VerifyInputMatchesAccountAndCollectionPeriod(y, _legalEntities[2]))), Times.Once);
        }

        private bool VerifyInputMatchesAccountAndCollectionPeriod(object functionInput, long accountLegalEntityId)
        {
            var accountLegalEntityAndCollectionPeriod = functionInput as AccountLegalEntityCollectionPeriod;
            if (accountLegalEntityAndCollectionPeriod == null)
            {
                return false;
            }

            return accountLegalEntityAndCollectionPeriod.AccountLegalEntityId == accountLegalEntityId && accountLegalEntityAndCollectionPeriod.CollectionPeriod == _collectionPeriod;
        }
    }
}