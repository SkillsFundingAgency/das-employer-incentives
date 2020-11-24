using AutoFixture;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentProcess.UnitTests
{
    public class WhenRunningPaymentsProcess
    {
        private Fixture _fixture;
        private CollectionPeriod _collectionPeriod;
        private Mock<IDurableOrchestrationContext> _mockOrchestrationContext;
        private IncentivePaymentOrchestrator _orchestrator;
        private List<PayableLegalEntityDto> _legalEntities;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();
            _collectionPeriod = _fixture.Create<CollectionPeriod>();
            _mockOrchestrationContext = new Mock<IDurableOrchestrationContext>();
            _mockOrchestrationContext.Setup(x => x.GetInput<CollectionPeriod>()).Returns(_collectionPeriod);

            _legalEntities = _fixture.CreateMany<PayableLegalEntityDto>(3).ToList();
            _mockOrchestrationContext.Setup(x => x.CallActivityAsync<List<PayableLegalEntityDto>>("GetPayableLegalEntities", _collectionPeriod)).ReturnsAsync(_legalEntities);

            _orchestrator = new IncentivePaymentOrchestrator(Mock.Of<ILogger<IncentivePaymentOrchestrator>>());
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
    }
}